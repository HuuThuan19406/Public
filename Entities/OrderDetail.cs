using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class OrderDetail
    {
        public OrderDetail()
        {
            NegotiationDetails = new HashSet<NegotiationDetail>();
            OrderDetailEditHistories = new HashSet<OrderDetailEditHistory>();
        }

        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public byte CategoryId { get; set; }
        public string FileUri { get; set; }
        public byte Quantity { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public string Note { get; set; }
        public DateTime? UploadedAt { get; set; }
        public bool IsAccepted { get; set; }

        public virtual Category Category { get; set; }
        public virtual Order Order { get; set; }
        public virtual ICollection<NegotiationDetail> NegotiationDetails { get; set; }
        public virtual ICollection<OrderDetailEditHistory> OrderDetailEditHistories { get; set; }
    }
}
