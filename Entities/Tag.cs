using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class Tag
    {
        public Tag()
        {
            GoodsTags = new HashSet<GoodsTag>();
            OrderTags = new HashSet<OrderTag>();
        }

        public string TagId { get; set; }

        public virtual ICollection<GoodsTag> GoodsTags { get; set; }
        public virtual ICollection<OrderTag> OrderTags { get; set; }
    }
}
