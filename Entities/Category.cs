using System;
using System.Collections.Generic;

#nullable disable

namespace Api.Entities
{
    public partial class Category
    {
        public Category()
        {
            Goods = new HashSet<Good>();
            InverseParentCategory = new HashSet<Category>();
            OrderDetails = new HashSet<OrderDetail>();
        }

        public byte CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public byte? ParentCategoryId { get; set; }

        public virtual Category ParentCategory { get; set; }
        public virtual ICollection<Good> Goods { get; set; }
        public virtual ICollection<Category> InverseParentCategory { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
