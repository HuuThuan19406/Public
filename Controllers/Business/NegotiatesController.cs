using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace Api.Controllers.Business
{
    /// <summary>Thương lượng Đơn Hàng - dành cho Người Bán.</summary>
    /// <response code="401">Chưa xác thực hoặc xác thực thất bại</response>
    /// <response code="409">Xung đột dữ liệu</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/business/[controller]")]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ApiController]
    [Authorize(Roles = "supplier")]
    public class NegotiatesController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Lấy thông tin Thương Lượng về Đơn Hàng <paramref name="orderId"/>.
        /// </summary>
        /// <param name="orderId"></param>
        /// <response code="200">Thành công và trả về thông tin.</response>
        /// <response code="423">Đơn Hàng này đã bị thu hồi hoặc lỗi nên không thể tương tác.</response>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(FullNegotiation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status423Locked)]
        public IActionResult Get([Required] int orderId)
        {
            var order = db.Orders.Find(orderId);

            if (order.IsDeleted || order.ProcessStatusId.Equals(8))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn Hàng này đã bị thu hồi hoặc lỗi nên không thể tương tác."
                }.Result();

            string supplierId = User.FindFirstValue(ClaimTypes.Sid).ToLower();
            var negotiation = db.Negotiations.Find(orderId, supplierId);
            var negotiationDetails = db.NegotiationDetails.Where(p => p.SupplierId.Equals(supplierId) && p.OrderDetail.OrderId.Equals(orderId));

            if (negotiation != null)
                negotiation.SetNullObjectChildren();

            return Ok(new FullNegotiation
            {
                Negotiation = negotiation,
                NegotiationDetails = negotiationDetails
            });
        }

        /// <summary>
        /// Cập nhật Thương lượng Đơn Hàng - dành cho Người Bán.
        /// </summary>
        /// <param name="negotiate">Chỉ cần truyền OrderId và MaxDurationByMinutes. Mọi trường còn lại truyền vào đều vô giá trị.</param>
        /// <response code="204">Thành công.</response>
        /// <response code="403">Không thể thao tác Đơn Hàng của người khác.</response>
        /// <response code="404">Không tìm thấy Đơn Hàng.</response>
        /// <response code="412">MaxDurationByMinutes không nằm trong giới hạn giá trị.</response>
        /// <response code="423">Đơn Hàng không thể thương lượng.</response>
        [HttpPut]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status423Locked)]
        public IActionResult Put([Required] Negotiation negotiate)
        {
            var order = db.Orders.Find(negotiate.OrderId);

            if (order == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy Đơn Hàng [{negotiate.OrderId}]."
                }.Result();

            if (order.IsDeleted)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn đã bị xóa, không thể thao tác."
                }.Result();

            if (order.ProcessStatusId.Equals(7) || order.ProcessStatusId.Equals(8))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn đã hoàn thành hoặc bị lỗi nên không thể thao tác."
                }.Result();

            if (order.ProcessStatusId > 3)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn đã được nhận nên không thể thao tác."
                }.Result();

            if (!order.SupplierId.Equals(User.FindFirstValue(ClaimTypes.Sid), StringComparison.OrdinalIgnoreCase))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Không thể thao tác Đơn Hàng của người khác."
                }.Result();

            if (negotiate.OrderMaxDurationByMinutes < Const.MINIMUM_ORDER_COMPLETION_DURATION.TotalMinutes)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status412PreconditionFailed,
                    Message = $"Thời Gian Hoàn Thành tối thiểu là {Const.MINIMUM_ORDER_COMPLETION_DURATION.TotalMinutes} phút."
                }.Result();

            if (negotiate.OrderMaxDurationByMinutes > Const.MAXIMUM_ORDER_COMPLETION_DURATION.TotalMinutes)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status412PreconditionFailed,
                    Message = $"Thời Gian Hoàn Thành tối đa là {Const.MAXIMUM_ORDER_COMPLETION_DURATION.TotalMinutes} phút."
                }.Result();

            negotiate.SupplierId = User.FindFirstValue(ClaimTypes.Sid).ToLower();

            var existNegotiate = db.Negotiations.Find(negotiate.OrderId, negotiate.SupplierId);

            if (existNegotiate != null)
            {
                existNegotiate.CreatedAt = DateTime.UtcNow;
                existNegotiate.OrderMaxDurationByMinutes = negotiate.OrderMaxDurationByMinutes;
                existNegotiate.Expired = negotiate.Expired;
            }
            else
            {
                negotiate.CreatedAt = DateTime.UtcNow;
                negotiate.SetNullObjectChildren();
            }

            try
            {
                if (existNegotiate != null)
                    db.Negotiations.Update(existNegotiate);
                else
                    db.Negotiations.Add(negotiate);

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

            return NoContent();
        }

        /// <summary>
        /// Chấp nhận hoặc từ chối với Thương Lượng của Đơn Hàng <paramref name="orderId"/>. Dù chấp nhận hay từ chối thì Thương Lượng cũng sẽ bị xóa.
        /// </summary>
        /// <param name="isAccept">true nếu đồng ý với Thương Lượng, false nếu từ chối.</param>
        /// <response code="204">Thành công.</response>
        /// <response code="404">Không tìm thấy thương lượng của Đơn Hàng.</response>
        /// <response code="408">Đã hết thời gian thương lượng hoặc Đơn Hàng đã hết hạn nên không thể thao tác.</response>
        /// <response code="423">Đơn Hàng này đã được nhận hoặc bị thu hồi hoặc lỗi nên không thể tương tác.</response>
        [HttpDelete("{orderId}")]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status408RequestTimeout)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status423Locked)]
        public IActionResult Delete([Required] int orderId, [Required] bool isAccept)
        {
            string supplierId = User.FindFirstValue(ClaimTypes.Sid).ToLower();
            var negotiation = db.Negotiations.Find(orderId, supplierId);
            var negotiationDetails = db
                .NegotiationDetails
                .Where(p => p.SupplierId.Equals(supplierId) && p.OrderDetail.OrderId.Equals(orderId))
                .Include(p => p.OrderDetail);

            if ((negotiation == null) && !negotiationDetails.Any())
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy thương lượng của Đơn Hàng [{orderId}]."
                }.Result();

            // negotiateDetails bắt buộc phải tạo hàng loạt, có tính ghi đè => Expired luôn bằng nhau.
            if ((negotiationDetails.Any() && (negotiationDetails.First().Expired < DateTime.UtcNow))
                || ((negotiation != null) && (negotiation.Expired < DateTime.UtcNow)))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status408RequestTimeout,
                    Message = "Đã hết thời gian thương lượng, không thể thao tác.",
                    RepairGuides = new string[]
                    {
                        "Nếu bạn vẫn còn muốn thương lượng Đơn Hàng này, bạn có thể tạo một Thương Lượng mới."
                    }
                }.Result();

            var order = db.Orders.Find(orderId);

            if (order.Expired < DateTime.UtcNow)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status408RequestTimeout,
                    Message = "Đơn Hàng đã hết hạn nên không thể thương lượng."
                }.Result();

            if (order.IsDeleted || order.ProcessStatusId.Equals(8))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn Hàng này đã bị thu hồi hoặc lỗi nên không thể tương tác."
                }.Result();

            if (order.ProcessStatusId > 3)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn đã được nhận nên không thể thao tác."
                }.Result();

            if (isAccept)
            {
                if (string.IsNullOrEmpty(order.SupplierId))
                    order.SupplierId = supplierId;

                order.ProcessStatusId = 4;

                if ((negotiation != null) && (negotiation.Expired >= DateTime.UtcNow))
                    order.MaxDurationByMinutes = negotiation.OrderMaxDurationByMinutes;

                if (negotiationDetails.Any() && (negotiationDetails.First().Expired >= DateTime.UtcNow))
                {
                    negotiationDetails.ForEachAsync((item) =>
                    {
                        item.OrderDetail.UnitPrice = item.OrderDetailUnitPrice;
                        item.OrderDetail.Quantity = item.OrderDetailQuantity;
                    }).Wait();
                }
            }

            try
            {
                db.Negotiations.Remove(negotiation);
                db.NegotiationDetails.RemoveRange(negotiationDetails);

                db.SaveChangesAsync().Wait();
            }
            catch (DbUpdateException ex)
            {
                return new StatusError
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = ex.Message
                }.Result();
            }

            return NoContent();
        }
    }
}