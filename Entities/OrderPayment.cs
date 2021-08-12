using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class OrderPayment
    {
        public int OrderId { get; set; }
        public byte PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Order Order { get; set; }
        public virtual Payment Payment { get; set; }
    }
}
