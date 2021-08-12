using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class LinkPage
    {
        public int LinkPageId { get; set; }
        public string SupplierId { get; set; }
        public string PageName { get; set; }
        public string Url { get; set; }

        public virtual Supplier Supplier { get; set; }
    }
}
