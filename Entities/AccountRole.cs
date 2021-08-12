using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class AccountRole
    {
        public string AccountId { get; set; }
        public byte RoleId { get; set; }

        public virtual Account Account { get; set; }
        public virtual Role Role { get; set; }
    }
}
