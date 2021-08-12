using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class OrderDetailEditHistory
    {
        public int OrderDetailEditHistoryId { get; set; }
        public int OrderDetailId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Requirement { get; set; }
        public string DecriptionFileUri { get; set; }

        public virtual OrderDetail OrderDetail { get; set; }
    }
}
