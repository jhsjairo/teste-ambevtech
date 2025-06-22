using Microsoft.AspNetCore.Mvc;
using OrderService.Application.UseCases;
using OrderService.Domain.Enums;

namespace OrderService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly GetOrdersByStatusUseCase _getOrdersByStatusUseCase;
        private readonly GetOrderByExternalIdUseCase _getOrderByExternalIdUseCase;

        public OrdersController(GetOrdersByStatusUseCase getOrdersByStatusUseCase, GetOrderByExternalIdUseCase getOrderByExternalIdUseCase)
        {
            _getOrdersByStatusUseCase = getOrdersByStatusUseCase;
            _getOrderByExternalIdUseCase = getOrderByExternalIdUseCase;
           
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] OrderStatus status = OrderStatus.Processed)
        {
            var orders = await _getOrdersByStatusUseCase.ExecuteAsync(status);

            var result = orders.Select(order => new
            {
                order.Id,
                order.ExternalOrderId,
                order.Status,
                order.CreatedAt,
                order.Total,
                Items = order.Items.Select(i => new
                {
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice,
                    Total = i.Total
                })
            });

            return Ok(result);
        }


        [HttpGet("{externalOrderId}")]
        public async Task<IActionResult> GetByExternalOrderId(string externalOrderId)
        {
            var order = await _getOrderByExternalIdUseCase.ExecuteAsync(externalOrderId);

            if (order == null)
                return NotFound();

            return Ok(new
            {
                order.Id,
                order.ExternalOrderId,
                order.Status,
                order.CreatedAt,
                order.Total,
                Items = order.Items.Select(i => new
                {
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice,
                    Total = i.Total
                })
            });
        }

    }
}
