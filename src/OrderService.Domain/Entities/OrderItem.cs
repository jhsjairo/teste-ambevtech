using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Total => Quantity * UnitPrice;

    protected OrderItem() { }

    public OrderItem(Guid orderId, string productName, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
        Quantity = quantity > 0 ? quantity : throw new ArgumentOutOfRangeException(nameof(quantity));
        UnitPrice = unitPrice >= 0 ? unitPrice : throw new ArgumentOutOfRangeException(nameof(unitPrice));
    }
}

