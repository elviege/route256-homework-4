using System.Runtime.CompilerServices;
using Dapper;
using Newtonsoft.Json;
using Npgsql;
using Orders.Core.Models;
using Microsoft.Extensions.Options;
using Orders.Core.Extensions;
using Orders.Core.Options;

namespace Orders.Core.Repositories;

public class OrdersRepository : IOrdersRepository
{
    private readonly NpgsqlConnection _connection;

    public OrdersRepository(IOptions<OrderServiceConfig> options)
    {
        _connection = new NpgsqlConnection(options.Value.ConnectionString);
        _connection.Open();
    }
    
    private const string FullSelectCmd =
        $"SELECT o.id Id, o.status_id StatusId, s.Name StatusName, o.client_id ClientId, c.Name ClientName, " +
        $"o.creation_dt CreationDt, o.issue_dt IssueDt, o.warehouse_id WarehouseId, w.Name WarehouseName, o.items_data ItemsData " +
        $"FROM orders o INNER JOIN statuses s ON o.status_id = s.id " +
        $"INNER JOIN clients c ON o.client_id = c.id " +
        $"INNER JOIN warehouses w ON o.warehouse_id = w.id";

    /*public async Task<IEnumerable<Order>> GetAll()
    {
        var commandText = $"SELECT o.id Id, o.status_id StatusId, o.client_id ClientId, o.creation_dt CreationDt, o.issue_dt IssueDt, o.warehouse_id WarehouseId, o.items_data ItemsData " +
                          $"FROM {TableName} o INNER JOIN statuses s ON o.status_id = s.id";
        var orders = await _connection.QueryAsync<Order, Status, Order>(commandText,
            (order, status) =>
            {
                order.Status = status;
                return order;
            }, splitOn: "StatusId");
        var commandText = $"SELECT * FROM {TableName}";
        var orders = await _connection.QueryAsync<Order>(commandText);
        return orders;
    }*/

    public async IAsyncEnumerable<Order> GetAll([EnumeratorCancellation] CancellationToken ct = default)
    {
        await using var cmd = new NpgsqlCommand(FullSelectCmd, _connection);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (!ct.IsCancellationRequested && await reader.ReadAsync(ct))
        {
            ct.ThrowIfCancellationRequested();
            var order = ReadOrderResponse(reader);
            yield return order;
        }
    }

    public async IAsyncEnumerable<Order> GetAllBy(long warehouseId, int statusId, DateTime createdDtStart, DateTime createdDtEnd,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (createdDtStart.Kind == DateTimeKind.Unspecified)
            createdDtStart = DateTime.SpecifyKind(createdDtStart, DateTimeKind.Utc);
        if (createdDtEnd.Kind == DateTimeKind.Unspecified)
            createdDtEnd = DateTime.SpecifyKind(createdDtEnd, DateTimeKind.Utc);
        
        var cmdText = FullSelectCmd + 
                      $" WHERE o.warehouse_id = {warehouseId} AND o.status_id = {statusId} " +
                      $"AND o.creation_dt BETWEEN '{createdDtStart.ToUniqueFormatString()}' AND '{createdDtEnd.ToUniqueFormatString()}'";
        await using var cmd = new NpgsqlCommand(cmdText, _connection);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (!ct.IsCancellationRequested && await reader.ReadAsync(ct))
        {
            ct.ThrowIfCancellationRequested();
            var order = ReadOrderResponse(reader);
            yield return order;
        }
    }

    public async Task<int> AddList(IEnumerable<Order> orders)
    {
        const string commandText = 
            $"INSERT INTO orders " +
            $"(id, client_Id, creation_dt, issue_dt, status_id, items_data, warehouse_id) " +
            $"VALUES (@Id, @ClientId, @CreationDt, @IssueDt, @StatusId, CAST(@ItemsData AS json), @WarehouseId)";
        var data = orders.Select(MapToInsertModel).Where(x => x is not null).ToArray();
        var inserted = 0;
        inserted += await _connection.ExecuteAsync(commandText, data);
        return inserted;
    }

    private Order ReadOrderResponse(NpgsqlDataReader reader)
    {
        var id = reader["Id"] as Guid?;
        var statusId = reader["StatusId"] as int?;
        var statusName = reader["StatusName"] as string;
        var clientId = reader["ClientId"] as long?;
        var clientName = reader["ClientName"] as string;
        var creationDt = reader["CreationDt"] as DateTime?;
        var issueDt = reader["IssueDt"] as DateTime?;
        var warehouseId = reader["WarehouseId"] as long?;
        var warehouseName = reader["WarehouseName"] as string;
        var itemsData = reader["ItemsData"] as string;
        
        ItemsDataModel itemsDataModel = null;
        if (!string.IsNullOrWhiteSpace(itemsData))
            itemsDataModel = JsonConvert.DeserializeObject<ItemsDataModel>(itemsData);

        if (creationDt.HasValue)
            creationDt = DateTime.SpecifyKind(creationDt.Value, DateTimeKind.Utc);
        if (issueDt.HasValue)
            issueDt = DateTime.SpecifyKind(issueDt.Value, DateTimeKind.Utc);
    
        return new Order
        {
            Id = id ?? Guid.Empty,
            Status = statusId.HasValue ? new Status{ Id = statusId.Value, Name = statusName } : null,
            Client = clientId.HasValue ? new Client{ Id = clientId.Value, Name = clientName } : null,
            CreationDt = creationDt ?? DateTime.MinValue,
            IssueDt = issueDt,
            Warehouse = warehouseId.HasValue ? new Warehouse { Id = warehouseId.Value, Name = warehouseName } : null,
            Items = itemsDataModel?.Items
        };
    }

    private OrderInsertModel MapToInsertModel(Order order)
    {
        if (order is null) return null;

        var dataModel = new ItemsDataModel { Items = order.Items };
        return new OrderInsertModel
        {
            Id = order.Id ?? Guid.NewGuid(),
            ClientId = order.Client?.Id ?? 0,
            CreationDt = order.CreationDt ?? DateTime.UtcNow,
            IssueDt = order.IssueDt,
            StatusId = order.Status?.Id ?? 0,
            WarehouseId = order.Warehouse?.Id ?? 0,
            ItemsData = JsonConvert.SerializeObject(dataModel)
        };
    }
}