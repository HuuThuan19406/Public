using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class Payment
    {
        public Payment()
        {
            OrderPayments = new HashSet<OrderPayment>();
        }

        public byte PaymentId { get; set; }
        public string PaymentName { get; set; }

        public virtual ICollection<OrderPayment> OrderPayments { get; set; }
    }
}
