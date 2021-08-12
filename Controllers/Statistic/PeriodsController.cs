using Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Api.Controllers.Statistic
{
    [Route("api/statistic/[controller]")]
    [ApiController]
    [Authorize(Roles = "administrator,statistician")]
    public class PeriodsController : ControllerBase
    {
        private BestsvContext db = new BestsvContext();

        [HttpOptions]
        public IEnumerable<Dictionary<DateTime, decimal>> Get([Required] string[] objectNames, [Required] DateTime from, [Required] DateTime to, [Required] byte stepByDay)
        {
            foreach (var item in objectNames)
            {
                switch (item.ToLower())
                {
                    case "order-revenue":
                        yield return OrderRevenue(from, to, stepByDay);
                        break;
                }
            }
        }

        private Dictionary<DateTime, decimal> OrderRevenue(DateTime from, DateTime to, byte stepByDay)
        {
            DateTime lastTime = from;
            var result = new Dictionary<DateTime, decimal>
            {
                {
                    from,
                    db.OrderDetails
                      .Where(p => (p.Order.CreatedAt <= lastTime))
                      .Sum(p => p.Quantity * p.UnitPrice)
                }
            };

            decimal accumulated = result.First().Value;

            from = from.AddDays(stepByDay);

            for (DateTime i = from; i < to; i = i.AddDays(stepByDay))
            {
                var value = db
                    .OrderDetails
                    .Where(p => (p.Order.CreatedAt >= lastTime) && (p.Order.CreatedAt <= i))
                    .Sum(p => p.Quantity * p.UnitPrice);

                accumulated += value;
                lastTime = i;
                result.Add(i, accumulated);
            }

            return result;
        }
    }
}
