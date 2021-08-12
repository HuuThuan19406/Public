using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Controllers.User
{
    /// <summary>
    /// Quản lý Tài Khoản - dành cho Người Dùng
    /// </summary>
    /// <response code="401">Chưa xác thực hoặc xác thực thất bại</response>
    /// <response code="409">Xung đột dữ liệu</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/user/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status409Conflict)]
    [Authorize]
    public class AccountsController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Lấy thông tin tài khoản cá nhân
        /// </summary>
        /// <response code="200">Thành công và trả về thông tin</response>
        [HttpGet]
        [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
        public Account Get()
        {
            var account = db.Accounts.Find(User.FindFirstValue(ClaimTypes.Sid));
            account.SetNullProperties("Password", "SecrectKey");

            return account;
        }

        /// <summary>
        /// Cập nhật thông tin Tài Khoản cá nhân. Ngoại trừ các kiểu dữ liệu nguyên thủy và DateTime, các kiểu dữ liệu còn lại đều không thể cập nhật.
        /// </summary>
        /// <param name="account">Thông tin cập nhật. Không truyền AccountId, Password, Email, SecrectKey, CreatedAt, LastLogin và IsDeleted. Mọi trường khác kiểu dữ liệu nguyên thủy và DateTime khi truyền vào đều không có giá trị (luôn bị bỏ qua).</param>
        /// <response code="200">Thành công và trả về thông tin đã cập nhật.</response>
        /// <response code="403">Không được chỉnh sửa dữ liệu của người khác.</response>
        /// <response code="404">Không tìm thấy Tài Khoản</response>
        [HttpPut]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public IActionResult Put([Required] Account account)
        {
            account.AccountId = User.FindFirstValue(ClaimTypes.Sid).ToLower();

            var findId = new BestsvContext().Accounts.FindAsync(account.AccountId);           

            if (findId.Result == null)
            {
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy tài khoản có id [{account.AccountId}]"
                }.Result();
            }
                        
            account.SetNullObjectChildren();
            account.Password = findId.Result.Password;
            account.SecrectKey = findId.Result.SecrectKey;
            account.CreatedAt = findId.Result.CreatedAt;
            account.LastLogin = findId.Result.LastLogin;
            account.IsDeleted = findId.Result.IsDeleted;

            try
            {
                db.Accounts.Update(account);
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

            return Ok(Get());
        }
    }
}