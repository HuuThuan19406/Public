using Api.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Api.Models
{
    public static class CategoryHandler
    {
        public static IEnumerable<byte> GetAllChildren(byte id)
        {
            List<byte> result = new List<byte>();
            List<byte> trace = new List<byte>();
            byte i = 0;

            BestsvContext db = new BestsvContext();

            result.AddRange(db.Categories.Where(p => p.ParentCategoryId.Value.Equals(id)).Select(p => p.CategoryId));

            while (i < result.Count)
            {
                if (!trace.Contains(result[i]))
                {
                    result.AddRange(db.Categories.Where(p => p.ParentCategoryId.Value.Equals(result[i])).Select(p => p.CategoryId));
                    trace.Add(result[i]);
                }
                i++;
            }

            return result;
        }
    }
}