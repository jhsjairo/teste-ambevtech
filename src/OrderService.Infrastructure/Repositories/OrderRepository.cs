using Dapper;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public OrderRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> ExistsAsync(string externalOrderId)
        {
            const string sql = "SELECT 1 FROM Orders WHERE ExternalOrderId = @externalOrderId";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<int?>(sql, new { externalOrderId }) == 1;
        }

        public async Task SaveAsync(Order order)
        {
            const string insertOrder = @"
            INSERT INTO Orders (Id, ExternalOrderId, Status, CreatedAt)
            VALUES (@Id, @ExternalOrderId, @Status, @CreatedAt)";

            const string insertItem = @"
            INSERT INTO OrderItems (Id, OrderId, ProductName, Quantity, UnitPrice)
            VALUES (@Id, @OrderId, @ProductName, @Quantity, @UnitPrice)";

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(insertOrder, new
            {
                order.Id,
                order.ExternalOrderId,
                Status = (int)order.Status,
                order.CreatedAt
            }, transaction);

            foreach (var item in order.Items)
            {
                await connection.ExecuteAsync(insertItem, new
                {
                    item.Id,
                    item.OrderId,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice
                }, transaction);
            }

            transaction.Commit();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            const string sql = @"
            SELECT * FROM Orders o
            LEFT JOIN OrderItems i ON o.Id = i.OrderId
            WHERE o.Status = @Status";

            using var connection = _connectionFactory.CreateConnection();
            var orderDictionary = new Dictionary<Guid, Order>();

            var result = await connection.QueryAsync<Order, OrderItem, Order>(
                sql,
                (order, item) =>
                {
                    if (!orderDictionary.TryGetValue(order.Id, out var existingOrder))
                    {
                        existingOrder = new Order(order.ExternalOrderId);
                        typeof(Order).GetProperty(nameof(Order.Id))!.SetValue(existingOrder, order.Id);
                        typeof(Order).GetProperty(nameof(Order.CreatedAt))!.SetValue(existingOrder, order.CreatedAt);
                        typeof(Order).GetProperty(nameof(Order.Status))!.SetValue(existingOrder, order.Status);
                        orderDictionary[existingOrder.Id] = existingOrder;
                    }

                    if (item != null)
                        existingOrder.Items.Add(item);

                    return existingOrder;
                },
                new { Status = (int)status },
                splitOn: "Id");

            return orderDictionary.Values;
        }

        public async Task<Order?> GetByExternalOrderIdAsync(string externalOrderId)
        {
            const string sql = @"
        SELECT * FROM Orders o
        LEFT JOIN OrderItems i ON o.Id = i.OrderId
        WHERE o.ExternalOrderId = @externalOrderId";

            using var connection = _connectionFactory.CreateConnection();
            var orderDictionary = new Dictionary<Guid, Order>();

            var result = await connection.QueryAsync<Order, OrderItem, Order>(
                sql,
                (order, item) =>
                {
                    if (!orderDictionary.TryGetValue(order.Id, out var existingOrder))
                    {
                        existingOrder = new Order(order.ExternalOrderId);
                        typeof(Order).GetProperty(nameof(Order.Id))!.SetValue(existingOrder, order.Id);
                        typeof(Order).GetProperty(nameof(Order.CreatedAt))!.SetValue(existingOrder, order.CreatedAt);
                        typeof(Order).GetProperty(nameof(Order.Status))!.SetValue(existingOrder, order.Status);
                        orderDictionary[existingOrder.Id] = existingOrder;
                    }

                    if (item != null)
                        existingOrder.Items.Add(item);

                    return existingOrder;
                },
                new { externalOrderId },
                splitOn: "Id");

            return orderDictionary.Values.FirstOrDefault();
        }


    }
}
