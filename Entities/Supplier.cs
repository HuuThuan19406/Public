using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class Supplier
    {
        public Supplier()
        {
            Certificates = new HashSet<Certificate>();
            Goods = new HashSet<Good>();
            LinkPages = new HashSet<LinkPage>();
            NegotiationDetails = new HashSet<NegotiationDetail>();
            Negotiations = new HashSet<Negotiation>();
            Orders = new HashSet<Order>();
        }

        public string SupplierId { get; set; }
        public short CollegeId { get; set; }
        public string Address { get; set; }
        public string Career { get; set; }
        public string FolderUri { get; set; }

        public virtual College College { get; set; }
        public virtual Account SupplierNavigation { get; set; }
        public virtual ICollection<Certificate> Certificates { get; set; }
        public virtual ICollection<Good> Goods { get; set; }
        public virtual ICollection<LinkPage> LinkPages { get; set; }
        public virtual ICollection<NegotiationDetail> NegotiationDetails { get; set; }
        public virtual ICollection<Negotiation> Negotiations { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
