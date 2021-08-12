using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class GoodsTag
    {
        public string GoodsId { get; set; }
        public string TagId { get; set; }

        public virtual Good Goods { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
