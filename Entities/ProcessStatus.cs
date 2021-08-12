using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class ProcessStatus
    {
        public ProcessStatus()
        {
            Orders = new HashSet<Order>();
        }

        public byte ProcessStatusId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
