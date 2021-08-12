using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class Account
    {
        public Account()
        {
            AccountRoles = new HashSet<AccountRole>();
            Orders = new HashSet<Order>();
            Purchases = new HashSet<Purchase>();
        }

        public string AccountId { get; set; }
        public byte? AvatarId { get; set; }
        public int ZipCodeId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? Sex { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string SecrectKey { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Avatar Avatar { get; set; }
        public virtual ZipCode ZipCode { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual ICollection<AccountRole> AccountRoles { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; }
    }
}
