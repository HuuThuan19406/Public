using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class ZipCode
    {
        public ZipCode()
        {
            Accounts = new HashSet<Account>();
        }

        public int ZipCodeId { get; set; }
        public string Position { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
    }
}
