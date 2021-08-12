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

namespace Api.Controllers.User
{
    ///<summary>Quản lý Đơn Đặt Hàng của Người Dùng (người mua).</summary>
    /// <response code="401">Chưa xác thực hoặc xác thực thất bại</response>
    /// <response code="409">Xung đột dữ liệu</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/user/[controller]")]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Lấy danh sách toàn bộ Đơn Đặt Hàng của Người Dùng theo điều kiện lọc (nếu có).
        /// </summary>
        /// <param name="skip">Vị trí dòng bắt đầu lấy dữ liệu.</param>
        /// <param name="take">Số lượng dòng dữ liệu lấy ra kể từ dòng <paramref name="skip"/></param>
        /// <param name="fromDay">Điều kiện ngày tạo hóa đơn CreatAt từ thời gian này đến hiện tại.</param>
        /// <param name="toDay">Điều kiện ngày tạo hóa đơn CreatAt từ thời gian này trở về trước.</param>
        /// <param name="isPending">Chỉ hiện Đơn Hàng công khai chưa có người nhận nếu giá trị truyền vào là true. Mặc định là true.</param>
        /// <response code="200">Thành công và trả về thông tin.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
        public IEnumerable<Order> Get([Required] int skip, [Required] int take, DateTime? fromDay, DateTime? toDay, bool isPending = true)
        {
            var filterFromDay = new Func<Order, bool>(p => fromDay.HasValue ? p.CreatedAt.AddMinutes(7) >= fromDay.Value : true);
            var filterToDay = new Func<Order, bool>(p => fromDay.HasValue ? p.CreatedAt.AddMinutes(7) <= toDay.Value : true);

            var data = db
                .Orders                
                .Where(p => isPending ? p.ProcessStatusId.Equals(1) : ((p.ProcessStatusId < 8) && !p.IsDeleted))
                .Where
                (
                    p => p
                        .AccountId
                        .Equals(User.FindFirstValue(ClaimTypes.Sid).ToLower())
                    & !p.IsDeleted
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
        /// Tạo Đơn Đặt Hàng. Mọi trường khác kiểu dữ liệu nguyên thủy và DateTime khi truyền vào đều không có giá trị (luôn bị bỏ qua).
        /// </summary>
        /// <param name="order">Thông tin Đơn Đặt Hàng. Chỉ cần truyền LimitEdit, Expired (quá hạn này sẽ không thể nhận đơn, thương lượng), MaxDurationByMinutes và OrderDetails (trường này sẽ không bị bỏ qua). Nếu muốn chỉ định Người Bán thì truyền thêm SupplierId. Trong OrderDetails không cần truyền OrderId, OrderDetailId và FileUri.</param>
        /// <response code="201">Thành công.</response>
        /// <response code="400">Thời Hạn Đơn Hàng nhỏ hơn Thời Gian Tạo Đơn hoặc giá trị trường nào đó không hợp lý.</response>
        /// <response code="403">Category của OrderDetail chưa phải là nguyên tử hoặc Người Mua chỉ định Người Bán là chính bản thân mình.</response>
        /// <response code="404">Không tìm thấy Người Bán.</response>
        /// <response code="412">MaxDurationByMinutes không nằm trong giới hạn giá trị.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status412PreconditionFailed)]
        public IActionResult Post([Required] Order order)
        {
            var orderDetails = order.OrderDetails.ToList();

            TaskArray taskArray = new TaskArray(orderDetails.Count() + 1);

            taskArray.AddAndStart(() =>
            {
                order.SetNullObjectChildren();
                order.SetNullProperties("OrderId", "DeliveryAt", "DoWorkAt", "DescriptionFileUri", "Tip");
            });
            foreach (var item in orderDetails)
            {
                taskArray.AddAndStart(() =>
                {
                    item.SetNullObjectChildren();
                    item.SetNullProperties("OrderId", "OrderDetailId", "FileUri");
                });
            }

            if (!string.IsNullOrEmpty(order.SupplierId))
            {
                if (order.SupplierId.Equals(User.FindFirstValue(ClaimTypes.Sid), StringComparison.OrdinalIgnoreCase))
                {
                    return new StatusError
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "Không thể chỉ định đơn hàng cho bản thân."
                    }.Result();
                }

                if (!db.Suppliers.Any(p => p.SupplierId.Equals(order.SupplierId.ToLower())))
                    return new StatusError
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = $"Không tìm thấy Người Bán [{order.SupplierId}]."
                    }.Result();
            }

            order.AccountId = User.FindFirstValue(ClaimTypes.Sid).ToLower();
            order.PaymentStatus = false;
            order.ProcessStatusId = 1;
            order.CommissionPercent = Const.COMMISSION_PERCENT_DEFAULT;
            order.CreatedAt = DateTime.UtcNow;
            order.ProcessStatusId = 8;
            order.IsDeleted = false;

            if (order.Expired < order.CreatedAt)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Thời Hạn Đơn Hàng không thể nhỏ hơn Thời Gian Tạo Đơn."
                }.Result();

            if (order.MaxDurationByMinutes < Const.MINIMUM_ORDER_COMPLETION_DURATION.TotalMinutes)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status412PreconditionFailed,
                    Message = $"Thời Gian Hoàn Thành tối thiểu là {Const.MINIMUM_ORDER_COMPLETION_DURATION.TotalMinutes} phút."
                }.Result();

            if (order.MaxDurationByMinutes > Const.MAXIMUM_ORDER_COMPLETION_DURATION.TotalMinutes)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status412PreconditionFailed,
                    Message = $"Thời Gian Hoàn Thành tối đa là {Const.MAXIMUM_ORDER_COMPLETION_DURATION.TotalMinutes} phút."
                }.Result();

            List<byte> categoryIdChecked = new List<byte>();
            foreach (var item in orderDetails)
            {
                if (categoryIdChecked.Contains(item.CategoryId))
                    continue;

                categoryIdChecked.Add(item.CategoryId);
                if (db.Categories.Any(p => p.ParentCategoryId.Equals(item.CategoryId)))
                    return new StatusError
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "Sản phẩm yêu cầu phải có Thể Loại là bậc dưới cùng, nghĩa là không thể chi tiết hơn được nữa."
                    }.Result();
            }

            taskArray.WaitAll();

            try
            {
                db.Orders.Add(order);
                db.SaveChanges();

                foreach (var item in orderDetails)
                {
                    item.OrderId = order.OrderId;
                }

                db.OrderDetails.AddRange(orderDetails);

                order.ProcessStatusId = (byte)(string.IsNullOrEmpty(order.SupplierId) ? 1 : 2);
                db.Orders.Update(order);

                db.SaveChangesAsync().Wait();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                return new StatusError
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = ex.Message
                }.Result();
            }

            order.OrderDetails = orderDetails.ToList();

            return Created("https://bestsv.net", order);
        }

        /// <summary>
        /// Đính kèm tệp mô tả yêu cầu vào Đơn Đặt Hàng <paramref name="id"/>. Nếu Đơn Đặt Hàng đã có tệp trước đó thì sẽ bị ghi đè, tệp trước đó sẽ bị xóa.
        /// </summary>
        /// <param name="id">OrderId.</param>
        /// <param name="fileName">Tên đầy đủ của tệp, bao gồm cả đuôi tệp.</param>
        /// <param name="dataFileBase64">Chuỗi Base64 của dữ liệu tệp.</param>
        /// <param name="signature">Chữ ký được tạo ra bằng cách băm <paramref name="dataFileBase64"/> bởi HmacSHA256 với Key là SecrectKey của Người Mua truyền vào.</param>
        /// <response code="204">Thành công.</response>
        /// <response code="400">Có thể do không thể xác minh tính toàn vẹn dữ liệu của tệp tải lên.</response>
        /// <response code="403">Không thể thao tác Đơn Hàng của người khác.</response>
        /// <response code="404">Không tìm thấy.</response>
        /// <response code="423">Đơn đã hoàn thành, bị lỗi hoặc đã thu hồi nên không thể thao tác.</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status423Locked)]
        public IActionResult Patch([Required] int id, [Required] string fileName, [Required, FromBody] string dataFileBase64, [Required] string signature)
        {
            var order = db.Orders.Find(id);

            if (order == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy"
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

            if (!order.AccountId.Equals(User.FindFirstValue(ClaimTypes.Sid), StringComparison.OrdinalIgnoreCase))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Không thể thao tác Đơn Hàng của người khác."
                }.Result();

            var hmac = new HmacSha256Simple(db.Accounts.Find(order.SupplierId).SecrectKey);

            if (hmac.IsIntegrity(dataFileBase64, signature.ToLower()) == false)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Không thể xác minh tính toàn vẹn dữ liệu của tệp tải lên."
                }.Result();

            var cloud = new GoogleDriveApi();
            var file = new GoogleDriveFileCreated(fileName, dataFileBase64, Const.CLOUD_ATTACH_FOLDER_URI, $"Tệp mô tả của Đơn Đặt Hàng [{id}]");

            if (!string.IsNullOrWhiteSpace(order.DescriptionFileUri))
            {
                try
                {
                    cloud.DeleteFileOrFolder(order.DescriptionFileUri);
                }
                catch { }
            }

            order.DescriptionFileUri = cloud.UploadFile(file);

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
        /// Kiểm duyệt sản phẩm <paramref name="orderDetailId"/> được tải lên. Khi hết số lần sửa LimitEdit hoặc toàn bộ sản phẩm đều được duyệt thì đơn sẽ hoàn tất.
        /// </summary>
        /// <param name="orderDetailId"></param>
        /// <param name="isAccept"></param>
        /// <param name="requirement"></param>
        /// 
        [HttpPatch("OrderDetails/{orderDetailId}")]
        public IActionResult Patch([Required] int orderDetailId, [Required] bool isAccept, string requirement)
        {
            if (!isAccept && string.IsNullOrWhiteSpace(requirement))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status412PreconditionFailed,
                    Message = "Để từ chối sản phẩm bạn cần nhập Phản Hồi để Người Bán khắc phục."
                }.Result();

            var orderDetail = db.OrderDetails.Find(orderDetailId);

            if (orderDetail == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy."
                }.Result();

            if (string.IsNullOrEmpty(orderDetail.FileUri))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Chưa có sản phẩm để kiểm duyệt."
                }.Result();

            if (orderDetail.IsAccepted)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Sản phẩm này đã được chấp nhận, không thể chỉnh sửa."
                }.Result();

            var order = db.Orders.Find(orderDetail.OrderId);
            var editedTimes = db.OrderDetailEditHistories.Count(p => p.OrderDetailId.Equals(orderDetailId));

            if (editedTimes >= order.LimitEdit)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Đơn Hàng đã đạt giới hạn chỉnh sửa, không thể kiểm duyệt."
                }.Result();

            if (order.IsDeleted)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn đã bị xóa, không thể thao tác."
                }.Result();

            if (!(order.ProcessStatusId.Equals(5) || order.ProcessStatusId.Equals(6)))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn Hàng không trong trạng thái có thể kiểm định sản phẩm."
                }.Result();

            if (!order.AccountId.Equals(User.FindFirstValue(ClaimTypes.Sid), StringComparison.OrdinalIgnoreCase))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Không thể thao tác Đơn Hàng của người khác."
                }.Result();

            orderDetail.IsAccepted = isAccept;
            db.OrderDetails.Update(orderDetail);

            if (isAccept == false)
            {
                db.OrderDetailEditHistories.Add(new OrderDetailEditHistory
                {
                    CreatedAt = DateTime.UtcNow,
                    OrderDetailId = orderDetailId,
                    Requirement = requirement
                });
            }

            if (db.OrderDetails.Any(p => 
                    p.OrderId.Equals(order.OrderId)
                    && !(p.IsAccepted
                        || (p.UploadedAt.HasValue
                            && (DateTime.UtcNow - p.UploadedAt.Value > Const.MAXIMUM_CHECK_ORDERDETAIL_DURATION))))
                && (++editedTimes < order.LimitEdit))
                order.ProcessStatusId = 6;
            else
                order.ProcessStatusId = 7;
            db.Orders.Update(order);

            try
            {
                db.SaveChangesAsync().Wait();
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
        /// Thu hồi Đơn Đặt Hàng theo <paramref name="id"/>
        /// </summary>
        /// <param name="id">OrderId</param>
        /// <response code="204">Thành công.</response>
        /// <response code="403">Không thể thao tác Đơn Đặt Hàng của người khác.</response>
        /// <response code="404">Không tìm thấy.</response>
        /// <response code="423">Đơn đã bị khóa, không thể thu hồi.</response>
        [HttpDelete("{id}")]
        public IActionResult Detele([Required] int id)
        {
            var order = db.Orders.Find(id);

            if (order == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy"
                }.Result();

            if (order.AccountId != User.FindFirstValue(ClaimTypes.Sid).ToLower())
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Không thể thao tác Đơn Đặt Hàng của người khác."
                }.Result();

            if (order.ProcessStatusId > 2)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn đã bị khóa, không thể thu hồi.",
                    RepairGuides = new string[]
                    {
                        "Đơn có thể đã có người nhận.",
                        "Đơn có thể đã hoàn thành.",
                        "Đơn có thể bị lỗi khi tạo."
                    }
                }.Result();

            order.IsDeleted = true;

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
    }
}