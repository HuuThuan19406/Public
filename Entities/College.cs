using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class College
    {
        public College()
        {
            Suppliers = new HashSet<Supplier>();
        }

        public short CollegeId { get; set; }
        public string CollegeName { get; set; }

        public virtual ICollection<Supplier> Suppliers { get; set; }
    }
}
