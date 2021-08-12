using Api.Entities;
using Api.Models;
using GoogleApi.Drive;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace Api.Controllers.Business
{
    ///<summary>Quản lý Đơn Đặt Hàng đã tiếp nhận - dành cho Người Bán.</summary>
    /// <response code="401">Chưa xác thực hoặc xác thực thất bại</response>
    /// <response code="409">Xung đột dữ liệu</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/business/[controller]")]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ApiController]
    [Authorize(Roles = "supplier")]
    public class OrdersController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();
        private readonly GoogleDriveApi cloud = new GoogleDriveApi();

        /// <summary>
        /// Lấy danh sách Đơn Đặt Hàng đã tiếp nhận của Người Bán theo điều kiện lọc (nếu có).
        /// </summary>
        /// <param name="skip">Vị trí dòng bắt đầu lấy dữ liệu.</param>
        /// <param name="take">Số lượng dòng dữ liệu lấy ra kể từ dòng <paramref name="skip"/></param>
        /// <param name="fromDay">Điều kiện ngày tạo Đơn Đặt Hàng CreatAt từ thời gian này đến hiện tại.</param>
        /// <param name="toDay">Điều kiện ngày tạo Đơn Đặt Hàng CreatAt từ thời gian này trở về trước.</param>
        /// <response code="200">Thành công và trả về thông tin.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
        public IEnumerable<Order> Get([Required] int skip, [Required] int take, DateTime? fromDay, DateTime? toDay)
        {
            var filterFromDay = new Func<Order, bool>(p => fromDay.HasValue ? p.CreatedAt.AddMinutes(7) >= fromDay.Value : true);
            var filterToDay = new Func<Order, bool>(p => fromDay.HasValue ? p.CreatedAt.AddMinutes(7) <= toDay.Value : true);

            var data = db
                .Orders
                .Where
                (
                    p => p
                        .SupplierId
                        .Equals(User.FindFirstValue(ClaimTypes.Sid).ToLower())
                    && (p.ProcessStatusId > 3)
                    && !p.IsDeleted
                )
                .Where(filterFromDay)
                .Where(filterToDay)
                .Skip(skip)
                .Take(take);

            foreach (var item in data)
            {
                item.SetNullProperties("DescriptionFileUri");
            }

            return data;
        }

        /// <summary>
        /// Tiếp nhận Đơn Đặt Hàng <paramref name="id"/>
        /// </summary>
        /// <param name="id">OrderId</param>
        /// <response code="204">Thành công.</response>
        /// <response code="403">Không thể nhận Đơn Hàng của bản thân.</response>
        /// <response code="404">Không tìm thấy.</response>
        /// <response code="405">Đơn này đang được thương lượng, thỏa hiệp thương lượng để nhận đơn.</response>
        /// <response code="408">Đơn Hàng đã hết hạn nhận đơn.</response>
        /// <response code="423">Đơn này không trong trạng thái có thể nhận. Có thể đơn đã có người nhận, đã chỉ định người nhận, đã hoàn thành, đã bị đóng bởi chủ sở hữu hoặc quá hạn.</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status405MethodNotAllowed)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status408RequestTimeout)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status423Locked)]
        public IActionResult Patch([Required] int id)
        {
            var order = db.Orders.Find(id);

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

            if (order.AccountId.Equals(User.FindFirstValue(ClaimTypes.Sid), StringComparison.OrdinalIgnoreCase))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Không thể nhận Đơn Hàng của bản thân."
                }.Result();

            if (order.Expired < DateTime.UtcNow)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status408RequestTimeout,
                    Message = "Đơn Hàng đã hết hạn nhận đơn."
                }.Result();

            if (order.ProcessStatusId.Equals(3))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status405MethodNotAllowed,
                    Message = "Đơn này đang được thương lượng, thỏa hiệp thương lượng để nhận đơn."
                }.Result();

            if (order.ProcessStatusId > 2)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn này không trong trạng thái có thể nhận.",
                    RepairGuides = new string[]
                    {
                        "Đơn này có thể đã có người nhận, đã chỉ định người nhận, đã hoàn thành hoặc đã bị đóng bởi chủ sở hữu."
                    }
                }.Result();

            if (order.ProcessStatusId.Equals(2) && !order.SupplierId.Equals(User.FindFirstValue(ClaimTypes.Sid), StringComparison.OrdinalIgnoreCase))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn này đã được chỉ định cho người khác."
                }.Result();

            order.SupplierId = User.FindFirstValue(ClaimTypes.Sid).ToLower();
            order.ProcessStatusId = 4;

            try
            {
                db.Orders.Update(order);
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
        /// Tải lên tệp sản phẩm tại OrderDetail theo <paramref name="orderDetailId"/>. Nếu đã có tệp trước đó thì sẽ bị ghi đè.
        /// </summary>
        /// <param name="orderDetailId"></param>
        /// <param name="fileName">Tên đầy đủ của tệp, bao gồm cả đuôi tệp.</param>
        /// <param name="dataFileBase64">Chuỗi Base64 của dữ liệu tệp.</param>
        /// <param name="signature">Chữ ký được tạo ra bằng cách băm <paramref name="dataFileBase64"/> bởi HmacSHA256 với Key là SecrectKey của Người Bán truyền vào.</param>
        /// <response code="201">Tải tệp thành công và trả về tên tệp.</response>
        /// <response code="400">Có thể do không thể xác minh tính toàn vẹn dữ liệu của tệp tải lên.</response>
        /// <response code="403">Không được phép thao tác Đơn Hàng của người khác.</response>
        /// <response code="404">Không tìm thấy Chi Tiết Đơn Hàng.</response>
        /// <response code="423">Đơn không trong trạng thái có thể đăng tải sản phẩm hoặc sản phẩm đã được chấp nhận, không thể chỉnh sửa.</response>
        [HttpPost("OrderDetails/{orderDetailId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status423Locked)]
        public IActionResult Post([Required] int orderDetailId, [Required] string fileName, [Required, FromBody] string dataFileBase64, [Required] string signature)
        {
            var orderDetail = db.OrderDetails.Find(orderDetailId);

            if (orderDetail == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy Chi Tiết Hóa Đơn [{orderDetail.OrderDetailId}]"
                }.Result();

            if (orderDetail.IsAccepted)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Sản phẩm đã dược chấp nhận, không thể chỉnh sửa."
                }.Result();

            var order = db.Orders.Find(orderDetail.OrderId);
            order.DeliveryAt = DateTime.UtcNow;

            if (order.IsDeleted)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn đã bị xóa, không thể thao tác."
                }.Result();

            if (!((order.ProcessStatusId >= 4) && (order.ProcessStatusId <= 6)))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn không trong trạng thái có thể đăng tải sản phẩm."
                }.Result();

            if (!order.SupplierId.Equals(User.FindFirstValue(ClaimTypes.Sid), StringComparison.OrdinalIgnoreCase))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Không được phép thao tác Đơn Hàng của người khác."
                }.Result();

            var hmac = new HmacSha256Simple(db.Accounts.Find(order.SupplierId).SecrectKey);

            if (hmac.IsIntegrity(dataFileBase64, signature.ToLower()) == false)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Không thể xác minh tính toàn vẹn dữ liệu của tệp tải lên."
                }.Result();

            var file = new GoogleDriveFileCreated(fileName, dataFileBase64, Const.CLOUD_ATTACH_FOLDER_URI, $"Tệp sản phẩm của Đơn Hàng [{order.OrderId}/{orderDetailId}].");

            if (!string.IsNullOrWhiteSpace(orderDetail.FileUri))
            {
                try
                {
                    cloud.DeleteFileOrFolder(orderDetail.FileUri);
                }
                catch { }
            }

            orderDetail.FileUri = cloud.UploadFile(file);
            order.ProcessStatusId = 5;

            try
            {
                db.Orders.Update(order);
                db.OrderDetails.Update(orderDetail);
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

            return StatusCode(201, fileName);
        }

        /// <summary>
        /// Lấy Tệp Mô Tả DescriptionFile của Đơn Hàng.
        /// </summary>
        /// <param name="id">orderId</param>
        /// <response code="200">Trả về tệp DescriptionFile.</response>
        /// <response code="403">Không được phép truy cập tệp mô tả này.</response>
        /// <response code="404">Không tìm thấy Đơn Hàng hoặc tệp mô tả.</response>
        /// <response code="423">Đơn đã bị xóa hoặc thu hồi.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GoogleDriveFileCreated), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status423Locked)]
        public IActionResult Get(int id)
        {
            var order = db.Orders.Find(id);
            var supplierId = User.FindFirstValue(ClaimTypes.Sid).ToLower();

            if (order == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy Đơn Hàng."
                }.Result();

            if (string.IsNullOrEmpty(order.DescriptionFileUri))
            {
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy tệp mô tả trong đơn hàng."
                }.Result();
            }

            if (order.IsDeleted)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn đã bị xóa, không thể thao tác."
                }.Result();

            if (order.ProcessStatusId.Equals(8))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn đã bị thu hồi, không thể thao tác."
                }.Result();

            if (order.IsDescriptionFilePrivate && (order.SupplierId != supplierId))
            {
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Tệp mô tả không được chia sẻ với bạn.",
                    RepairGuides = new[]
                    {
                        "Cần chủ đơn hàng mời nhận đơn.",
                        "Chuyển tệp mô tả thành công khai."
                    }
                }.Result();
            }

            var descriptionFile = cloud.DownloadFile(order.DescriptionFileUri);

            return Ok(descriptionFile);
        }
    }
}
