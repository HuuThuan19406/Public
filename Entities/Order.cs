using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class Order
    {
        public Order()
        {
            Negotiations = new HashSet<Negotiation>();
            OrderDetails = new HashSet<OrderDetail>();
            OrderEvaluations = new HashSet<OrderEvaluation>();
            OrderPayments = new HashSet<OrderPayment>();
            OrderTags = new HashSet<OrderTag>();
        }

        public int OrderId { get; set; }
        public string AccountId { get; set; }
        public string SupplierId { get; set; }
        public byte ProcessStatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime Expired { get; set; }
        public short MaxDurationByMinutes { get; set; }
        public DateTime? DoWorkAt { get; set; }
        public DateTime? DeliveryAt { get; set; }
        public byte LimitEdit { get; set; }
        public bool PaymentStatus { get; set; }
        public string DescriptionFileUri { get; set; }
        public bool IsDescriptionFilePrivate { get; set; }
        public string DescriptionText { get; set; }
        public decimal CommissionPercent { get; set; }
        public decimal? Tip { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Account Account { get; set; }
        public virtual ProcessStatus ProcessStatus { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual ICollection<Negotiation> Negotiations { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<OrderEvaluation> OrderEvaluations { get; set; }
        public virtual ICollection<OrderPayment> OrderPayments { get; set; }
        public virtual ICollection<OrderTag> OrderTags { get; set; }
    }
}
