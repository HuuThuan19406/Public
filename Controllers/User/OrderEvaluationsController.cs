using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace Api.Controllers.User
{
    ///<summary>Quản lý đánh giá Đơn Hàng đã tiếp nhận - dành cho Người Mua.</summary>
    /// <response code="401">Chưa xác thực hoặc xác thực thất bại</response>
    /// <response code="409">Xung đột dữ liệu</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/user/[controller]")]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ApiController]
    [Authorize]
    public class OrderEvaluationsController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Người Mua đánh giá Người Bán thông qua Đơn Hàng.
        /// </summary>
        /// <param name="orderEvaluation">Chỉ cần truyền OrderId, Rate và Comment (nếu có).</param>
        /// <response code="201">Thành công.</response>
        /// <response code="400">Giá trị Rate không phù hợp.</response>
        /// <response code="403">Không thể đánh giá Đơn Hàng của người khác hoặc Đơn Hàng này đã được đánh giá.</response>
        /// <response code="404">Không tìm thấy Đơn Hàng.</response>
        /// <response code="423">Đơn Hàng chưa hoàn tất hoặc đã bị thu hồi.</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrderEvaluation), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status423Locked)]
        public IActionResult Post([Required] OrderEvaluation orderEvaluation)
        {
            if (orderEvaluation.Rate > 50)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Giá trị Rate không quá 50."
                }.Result();

            if (orderEvaluation.Rate % 5 != 0)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Giá trị Rate phải chia hết cho 5."
                }.Result();

            var order = db.Orders.Find(orderEvaluation.OrderId);

            if (order == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy."
                }.Result();

            if (order.IsDeleted)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn đã bị xóa, không thể thao tác."
                }.Result();

            if (!order.ProcessStatusId.Equals(7))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn hoàn tất mới có thể đánh giá."
                }.Result();

            if (!order.AccountId.Equals(User.FindFirstValue(ClaimTypes.Sid), StringComparison.OrdinalIgnoreCase))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Không thể đánh giá Đơn Hàng của người khác."
                }.Result();

            if (db.OrderEvaluations.Any(p => p.OrderId.Equals(orderEvaluation.OrderId) && p.Type.Equals(false)))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Đơn Hàng này đã được đánh giá rồi, không thể đánh giá lại."
                }.Result();

            orderEvaluation.Type = false;
            orderEvaluation.CreatedAt = DateTime.UtcNow;

            try
            {
                db.OrderEvaluations.Add(orderEvaluation);
                db.SaveChanges();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                return new StatusError
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = ex.Message
                }.Result();
            }

            orderEvaluation.SetNullObjectChildren();

            return Created("https://bestsv.net", orderEvaluation);
        }
    }
}