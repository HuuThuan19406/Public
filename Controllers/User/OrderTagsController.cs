using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace Api.Controllers.User
{
    /// <summary>
    /// Quản lý thẻ Tag của Đơn Đặt Hàng - dành cho Người Dùng
    /// </summary>
    /// <response code="401">Chưa xác thực hoặc xác thực thất bại</response>
    /// <response code="409">Xung đột dữ liệu</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/user/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status409Conflict)]
    [Authorize]
    public class OrderTagsController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Trả về danh sách giá trị thẻ Tag của Đơn Đặt Hàng <paramref name="orderId"/>
        /// </summary>
        /// <param name="orderId"></param>
        /// <response code="200">Thành công và trả về thông tin.</response>
        /// <response code="403">Không thể xem Đơn Đặt Hàng của người khác.</response>
        /// <response code="404">Không tìm thấy Đơn Đặt Hàng.</response>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public IActionResult Get([Required] int orderId)
        {
            var order = db.Orders.Find(orderId);

            if (order == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy Đơn Đặt Hàng [{orderId}]"
                }.Result();

            if (order.AccountId != User.FindFirstValue(ClaimTypes.Sid).ToLower())
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Không thể xem Đơn Đặt Hàng của người khác."
                }.Result();

            return Ok(db.OrderTags.Where(p => p.OrderId.Equals(orderId)).Select(o => o.TagId));
        }

        /// <summary>
        /// Gắn thẻ <paramref name="tagId"/> cho Đơn Đặt Hàng <paramref name="orderId"/>
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="tagId">Tên thẻ Tag muốn gắn. Không phân biệt hoa thường.</param>
        /// <response code="201">Thành công.</response>
        /// <response code="400">Thẻ Tag không đúng định dạng.</response>
        /// <response code="403">Thẻ Tag bị trùng hoặc không được chỉnh sửa tài nguyên của người khác.</response>
        /// <response code="404">Không tìm thấy Đơn Đặt Hàng.</response>
        [HttpPost("{orderId}/{tagId}")]
        [ProducesResponseType(typeof(OrderTag), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public IActionResult Post([Required] int orderId, [Required] string tagId)
        {
            if (!tagId.IsTagVaild())
                return new StatusError
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Tag không đúng định dạng.",
                    RepairGuides = new string[]
                    {
                        "Tag chỉ gồm kí tự chữ Latin không phân biệt hoa thường, kí tự số, kí tự gạch chân (_) và kí tự gạch nối (-)."
                    }
                }.Result();

            if (!db.Orders.Any(p => p.OrderId.Equals(orderId)))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy Đơn Đặt Hàng [{orderId}]"
                }.Result();

            if (!db.Orders.Any(p => p.OrderId.Equals(orderId) && p.AccountId.Equals(User.FindFirstValue(ClaimTypes.Sid).ToLower())))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Không thể gắn Tag cho Đơn Đặt Hàng của người khác."
                }.Result();

            tagId = tagId.ToLower();

            if (db.OrderTags.Any(p => p.OrderId.Equals(orderId) && p.TagId.Equals(tagId)))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = $"Thẻ {tagId.ToLower()} đã được gắn cho đơn này rồi, không thể ghi trùng. Thẻ không phân biệt hoa thường."
                }.Result();

            if (!db.Tags.Any(p => p.TagId.Equals(tagId)))
            {
                try
                {
                    db.Tags.Add(new Tag { TagId = tagId });
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
            }

            var orderTag = new OrderTag
            {
                OrderId = orderId,
                TagId = tagId
            };

            try
            {
                db.OrderTags.Add(orderTag);
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

            return Created("https://bestsv.net", orderTag);
        }

        /// <summary>
        /// Xóa thẻ Tag <paramref name="tagId"/> khỏi Đơn Đặt Hàng <paramref name="orderId"/>
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="tagId"></param>
        /// <response code="204">Thành công.</response>
        /// <response code="403">Không được chỉnh sửa tài nguyên của người khác.</response>
        /// <response code="404">Không tìm thấy Đơn Đặt Hàng hoặc thẻ Tag trong đơn.</response>
        [HttpDelete("{orderId}/{tagId}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public IActionResult Delete([Required] int orderId, [Required] string tagId)
        {
            var orderTag = db.OrderTags.Find(orderId, tagId.ToLower());

            if (orderTag == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy Đơn Đặt Hàng hoặc thẻ Tag trong đơn."
                }.Result();

            if (!db.Orders.Any(p => p.OrderId.Equals(orderId) && p.AccountId.Equals(User.FindFirstValue(ClaimTypes.Sid).ToLower())))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Không thể xóa Tag thuộc Đơn Đặt Hàng của người khác."
                }.Result();

            try
            {
                db.OrderTags.Remove(orderTag);
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
    }
}