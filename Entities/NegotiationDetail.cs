using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class NegotiationDetail
    {
        public int OrderDetailId { get; set; }
        public string SupplierId { get; set; }
        public byte OrderDetailQuantity { get; set; }
        public decimal OrderDetailUnitPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Expired { get; set; }

        public virtual OrderDetail OrderDetail { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
