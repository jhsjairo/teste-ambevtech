using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Enums
{
    public enum OrderStatus
    {
        Processed = 1,
        Sent = 2,
        Cancelled = 3
    }
}
