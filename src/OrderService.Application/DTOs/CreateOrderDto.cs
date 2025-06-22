using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTOs
{
    public class CreateOrderDto
    {
        public string ExternalOrderId { get; set; } = string.Empty;
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
