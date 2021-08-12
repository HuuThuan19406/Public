using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class RefreshToken
    {
        public Guid RefreshTokenId { get; set; }
        public string AccountId { get; set; }
        public string Ipaddress { get; set; }
        public DateTime Expired { get; set; }
    }
}
