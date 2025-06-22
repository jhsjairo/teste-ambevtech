using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces
{

    public interface IOrderRepository
    {
        Task<bool> ExistsAsync(string externalOrderId);
        Task SaveAsync(Order order);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<Order?> GetByExternalOrderIdAsync(string externalOrderId);
    }
}
