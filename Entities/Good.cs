using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class Good
    {
        public Good()
        {
            GoodsTags = new HashSet<GoodsTag>();
            Purchases = new HashSet<Purchase>();
        }

        public string GoodsId { get; set; }
        public string SupplierId { get; set; }
        public byte CategoryId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string Note { get; set; }
        public DateTime UploadDate { get; set; }

        public virtual Category Category { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual ICollection<GoodsTag> GoodsTags { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; }
    }
}
