using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class Identification
    {
        public string IdentificationId { get; set; }
        public string Pin { get; set; }
        public DateTime Expired { get; set; }
    }
}
