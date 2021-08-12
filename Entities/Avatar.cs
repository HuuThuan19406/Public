using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class Avatar
    {
        public Avatar()
        {
            Accounts = new HashSet<Account>();
        }

        public byte AvatarId { get; set; }
        public string Uri { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
    }
}
