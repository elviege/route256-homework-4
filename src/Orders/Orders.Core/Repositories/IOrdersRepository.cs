using System.Runtime.CompilerServices;
using Orders.Core.Models;

namespace Orders.Core.Repositories;

public interface IOrdersRepository
{
    IAsyncEnumerable<Order> GetAll([EnumeratorCancellation] CancellationToken ct = default);
    IAsyncEnumerable<Order> GetAllBy(long warehouseId, int statusId, DateTime createdDtStart, DateTime createdDtEnd, 
        [EnumeratorCancellation] CancellationToken ct = default);
    Task<int> AddList(IEnumerable<Order> orders);
}