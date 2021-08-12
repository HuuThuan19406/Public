using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Api.Controllers.Public
{
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/public/[controller]")]
    [ApiController]
    public class OrderEvaluationsController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Tính điểm Rate trung bình Đánh Giá Đơn Hàng của Người Mua <paramref name="accountId"/>. Nếu không tìm thấy Người Mua vẫn không báo lỗi.
        /// </summary>
        /// <param name="accountId"></param>
        /// <response code="200">Thành công và trả về thông tin.</response>
        [HttpGet("account/{accountId}")]
        [ProducesResponseType(typeof(RateStatistics), StatusCodes.Status200OK)]
        public RateStatistics GetAccount([Required] string accountId)
        {
            var data = db
                .OrderEvaluations
                .Where(p => p.Order.AccountId == accountId.ToLower());

            if (data.Any())
                return new RateStatistics
                {
                    AverageRate = data.Average(p => p.Rate),
                    Count = data.Count()
                };

            return new RateStatistics
            {
                AverageRate = double.NaN,
                Count = 0
            };
        }

        /// <summary>
        /// Tính điểm Rate trung bình Đánh Giá Đơn Hàng của Người Bán <paramref name="supplierId"/>. Nếu không tìm thấy Người Bán vẫn không báo lỗi.
        /// </summary>
        /// <param name="supplierId"></param>
        /// <response code="200">Thành công và trả về thông tin.</response>
        [HttpGet("supplier/{supplierId}")]
        [ProducesResponseType(typeof(RateStatistics), StatusCodes.Status200OK)]
        public RateStatistics GetSupplier([Required] string supplierId)
        {
            var data = db
                .OrderEvaluations
                .Where(p => p.Order.SupplierId == supplierId.ToLower());

            if (data.Any())
                return new RateStatistics
                {
                    AverageRate = data.Average(p => p.Rate),
                    Count = data.Count()
                };

            return new RateStatistics
            {
                AverageRate = double.NaN,
                Count = 0
            };
        }

        /// <summary>
        /// Lấy thông tin đánh giá của Đơn Hàng <paramref name="orderId"/> loại <paramref name="type"/>.
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="type">true: Người Bán đánh giá Người Mua, false: Người Mua đánh giá Người Bán.</param>
        /// <response code="200">Thành công và trả về thông tin.</response>
        /// <response code="404">Không tìm thấy.</response>
        [HttpGet("{orderId}/{type}")]
        [ProducesResponseType(typeof(OrderEvaluation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public IActionResult Get([Required] int orderId, [Required] bool type)
        {
            var orderEvaluation = db.OrderEvaluations.Find(orderId, type);

            if (orderEvaluation == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy."
                }.Result();

            return Ok(orderEvaluation);
        }
    }
}
