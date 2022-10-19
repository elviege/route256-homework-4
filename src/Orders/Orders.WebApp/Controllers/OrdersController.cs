using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Orders.Core.Models;
using Orders.Core.Options;
using Orders.Core.Repositories;

namespace Orders.WebApp.Controllers;

[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private readonly OrderServiceConfig _config;
    private readonly IOrdersRepository _repository;
    
    public OrdersController(ILogger<OrdersController> logger, IOptions<OrderServiceConfig> options, IOrdersRepository repository)
    {
        _logger = logger;
        _config = options.Value;
        _repository = repository;
    }

    [HttpGet("GetAll")]
    public IAsyncEnumerable<Order> GetAll()
    {
        var ct = new CancellationToken();
        var orders = _repository.GetAll(ct);
        return orders;
    }
    
    [HttpGet("GetAllBy/{warehouseId}/{statusId}/{creationDtStart}/{creationDtEnd}")]
    public IAsyncEnumerable<Order> GetAllBy(long warehouseId, int statusId, DateTime creationDtStart, DateTime creationDtEnd)
    {
        var ct = new CancellationToken();
        var orders = _repository.GetAllBy(warehouseId, statusId, creationDtStart, creationDtEnd, ct);
        return orders;
    }

    [HttpPost("AddList")]
    public async Task<ActionResult> AddList(OrdersAddRequestModel model)
    {
        if (model is null || model.Data is null)
            return Ok("empty list in request");
        
        var count = await _repository.AddList(model.Data);
        return Ok($"{count} items success!");
    }
}