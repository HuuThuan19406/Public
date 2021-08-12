using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class Purchase
    {
        public string GoodsId { get; set; }
        public string AccountId { get; set; }
        public byte PaymentId { get; set; }
        public DateTime BoughtAt { get; set; }

        public virtual Account Account { get; set; }
        public virtual Good Goods { get; set; }
    }
}
