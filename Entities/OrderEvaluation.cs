using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class OrderEvaluation
    {
        public int OrderId { get; set; }
        public bool Type { get; set; }
        public byte Rate { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Order Order { get; set; }
    }
}
