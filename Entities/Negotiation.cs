using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class Negotiation
    {
        public int OrderId { get; set; }
        public string SupplierId { get; set; }
        public short OrderMaxDurationByMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte OrderLimitEdit { get; set; }
        public DateTime? Expired { get; set; }

        public virtual Order Order { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
