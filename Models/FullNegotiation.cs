using Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
    public class FullNegotiation
    {
        public Negotiation Negotiation { get; set; }
        public IEnumerable<NegotiationDetail> NegotiationDetails { get; set; }
    }
}
