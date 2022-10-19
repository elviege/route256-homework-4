using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Orders.Core.Models;
using Orders.Core.Repositories;

namespace Orders.GrpcService.Services;

public class OrderService : Order.OrderBase
{
    private readonly ILogger<OrderService> _logger;
    private readonly IOrdersRepository _repository;

    public OrderService(ILogger<OrderService> logger, IOrdersRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public override async Task GetAll(Empty request, 
        IServerStreamWriter<OrderMessage> responseStream, 
        ServerCallContext context)
    {
        try
        {
            var orders = _repository.GetAll(context.CancellationToken);
            await foreach (var order in orders.WithCancellation(context.CancellationToken))
            {
                var result = MapToOrderMessage(order);
                await responseStream.WriteAsync(result);
                await Task.Delay(100, context.CancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning($"{nameof(OrderService.GetAll)} operation was canceled");
        }
    }

    public override async Task GetAllBy(OrderRequest request, 
        IServerStreamWriter<OrderMessage> responseStream, 
        ServerCallContext context)
    {
        try
        {
            var orders = _repository.GetAllBy(request.WarehouseId, request.StatusId, 
                request.CreationDtStart.ToDateTime(), request.CreationDtEnd.ToDateTime(), context.CancellationToken);
            await foreach (var order in orders.WithCancellation(context.CancellationToken))
            {
                var result = MapToOrderMessage(order);
                await responseStream.WriteAsync(result);
                await Task.Delay(100, context.CancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning($"{nameof(OrderService.GetAllBy)} operation was canceled");
        }
    }

    public override Task<OrderAddResponse> AddList(OrderAddRequest request, ServerCallContext context)
    {
        var count = 0;
        try
        {
            var data = request.Data.Select(MapToOrderModel)
                .Where(d => d is not null).ToArray();
            var task = _repository.AddList(data);
            count = task.Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(AddList));
        }
        return Task.FromResult(new OrderAddResponse { Count = count});
    }

    private OrderMessage MapToOrderMessage(Core.Models.Order order)
    {
        var resp = new OrderMessage
        {
            Id = order.Id.ToString(),
            Status = new Status { Id = order.Status.Id, Name = order.Status.Name },
            Client = new Client { Id = order.Client.Id, Name = order.Client.Name },
            CreationDt = order.CreationDt?.ToTimestamp(),
            IssueDt = order.IssueDt?.ToTimestamp(),
            Warehouse = new Warehouse { Id = order.Warehouse.Id, Name = order.Warehouse.Name }
        };
        foreach (var item in order.Items)
        {
            resp.Items.Add(new Item
            {
                ItemId = item.ItemId,
                Count = item.Count
            });
        }
        
        return resp;
    }

    private Core.Models.Order MapToOrderModel(OrderMessage message)
    {
        if (message is null) return null;

        Guid? id = null;
        if (Guid.TryParse(message.Id, out Guid uid))
            id = uid;
        
        return new Core.Models.Order
        {
            Id = id,
            Client = message.Client is null ? null : new Core.Models.Client { Id = message.Client.Id, Name = message.Client.Name },
            CreationDt = message.CreationDt?.ToDateTime(),
            IssueDt = message.IssueDt?.ToDateTime(),
            Status = message.Status is null ? null : new Core.Models.Status { Id = message.Status.Id, Name = message.Status.Name },
            Items = message.Items.Select(item => new Core.Models.Item { ItemId = item.ItemId, Count = item.Count }).ToArray(),
            Warehouse = message.Warehouse is null ? null : new Core.Models.Warehouse { Id = message.Warehouse.Id, Name = message.Warehouse.Name }
        };
    }
}