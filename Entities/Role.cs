using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class Role
    {
        public Role()
        {
            AccountRoles = new HashSet<AccountRole>();
        }

        public byte RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<AccountRole> AccountRoles { get; set; }
    }
}
