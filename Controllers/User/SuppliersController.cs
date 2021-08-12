using Api.Entities;
using Api.Models;
using GoogleApi.Drive;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace Api.Controllers.User
{
    /// <response code="401">Chưa xác thực hoặc xác thực thất bại</response>
    /// <response code="409">Xung đột dữ liệu</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/user/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status409Conflict)]
    [Authorize]
    public class SuppliersController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Đăng ký trở thành Người Bán.
        /// </summary>
        /// <param name="supplier">Thông tin cần thiết để đăng ký. Không cần truyền SupplierId. FolderId truyền chuỗi bất kỳ để không lỗi. Mọi trường khác kiểu dữ liệu nguyên thủy và DateTime khi truyền vào đều không có giá trị (luôn bị bỏ qua).</param>
        /// <response code="201">Thành công và trả về thông tin Người Bán. Cần làm mới Token để cập nhật Role - Quyền Truy Cập Api.</response>
        /// <response code="412">Tài Khoản này đã đăng kí trở thành Người Bán trước đây rồi.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Supplier), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status412PreconditionFailed)]
        public IActionResult Post([Required] Supplier supplier)
        {
            supplier.SupplierId = User.FindFirstValue(ClaimTypes.Sid).ToLower();

            if (db.Suppliers.Any(p => p.SupplierId.Equals(supplier.SupplierId)))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status412PreconditionFailed,
                    Message = "Tài Khoản này đã đăng kí trở thành Người Bán rồi, không cần phải đăng ký lại."
                }.Result();

            supplier.SetNullObjectChildren();

            try
            {
                db.Suppliers.Add(supplier);
                db.AccountRoles.Add(new AccountRole
                {
                    AccountId = supplier.SupplierId,
                    RoleId = 4
                });

                db.SaveChangesAsync().Wait();

                GoogleDriveApi driveApi = new GoogleDriveApi();
                supplier.FolderUri = driveApi.CreateFolder(supplier.SupplierId, Const.CLOUD_SUPPLIER_FOLDER_URI);

                db.Suppliers.Update(supplier);
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

            return Created("https://bestsv.net", supplier);
        }
    }
}