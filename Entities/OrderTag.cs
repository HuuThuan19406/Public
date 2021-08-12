using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class OrderTag
    {
        public int OrderId { get; set; }
        public string TagId { get; set; }

        public virtual Order Order { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
