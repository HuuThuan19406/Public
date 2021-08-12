using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers.Admin
{
    /// <summary>
    /// Quản lý Tài Khoản - dành cho Quản Trị Viên. Trang dành cho Người Dùng tại api/user/Account
    /// </summary>
    /// <response code="401">Chưa xác thực hoặc xác thực thất bại</response>
    /// <response code="409">Xung đột dữ liệu</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/admin/[controller]")]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ApiController]
    [Authorize(Roles = "administrator")]
    public class AccountsController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Lấy ra danh sách toàn bộ Tài Khoản
        /// </summary>
        /// <param name="skip">Vị trí dòng bắt đầu lấy dữ liệu.</param>
        /// <param name="take">Số lượng dòng dữ liệu lấy ra kể từ dòng <paramref name="skip"/>.</param>
        /// <param name="filter">Email, di động hoặc tên người dùng, không phân biệt hoa thường.</param>
        /// <response code="200">Lấy ra danh sách toàn bộ Tài Khoản không bao gồm thuộc tính kiểu đối tượng (trừ DateTime)</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Account>), StatusCodes.Status200OK)]
        public IEnumerable<Account> Get([Required] int skip, [Required] int take, string filter = "")
        {
            filter = filter.ToLower();
            var data = db.Accounts
                .Where(p => p.UserName.Contains(filter)
                    || (p.FirstName + ' ' + p.LastName).Contains(filter)
                    || p.AccountId.Contains(filter)
                    || p.Phone.Contains(filter))
                .Skip(skip)
                .Take(take)
                .ToHashSet();

            TaskArray taskArray = new TaskArray(data.Count);

            foreach (var item in data)
            {
                taskArray.AddAndStart(() => item.SetNullProperties("Password", "SecrectKey"));
            }

            taskArray.WaitAll();

            return data;
        }

        /// <summary>
        /// Lấy thông tin Tài Khoản theo <paramref name="id"/>
        /// </summary>
        /// <response code="200">Trả về thông tin Tài Khoản không bao gồm thuộc tính kiểu đối tượng (trừ DateTime)</response>
        /// <response code="404">Không tìm thấy Tài Khoản</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAsync([Required, EmailAddress] string id)
        {
            var account = await db.Accounts.FindAsync(id.ToLower());

            if (account == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy tài khoản [{id}]"
                }.Result();

            account.SetNullProperties("Password");

            return Ok(account);
        }

        /// <summary>
        /// Cập nhật thông tin Tài Khoản theo <paramref name="id"/>. Ngoại trừ các kiểu dữ liệu nguyên thủy và DateTime, các kiểu dữ liệu còn lại đều không thể cập nhật.
        /// </summary>
        /// <param name="id">accountId</param>
        /// <param name="account">Thông tin cập nhật. Không truyền AccountId, Password, Email, SecrectKey, CreatedAt, LastLogin và IsDeleted. Mọi trường khác kiểu dữ liệu nguyên thủy và DateTime khi truyền vào đều không có giá trị (luôn bị bỏ qua).</param>
        /// <response code="204">Thành công</response>
        /// <response code="403">Không được chỉnh sửa dữ liệu của người khác.</response>
        /// <response code="404">Không tìm thấy Tài Khoản</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public IActionResult Put([Required, EmailAddress] string id, [Required] Account account)
        {
            account.AccountId = id.ToLower();

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

            return NoContent();
        }

        /// <summary>
        /// Đánh dấu Tài Khoản là đã bị xóa.
        /// </summary>
        /// <param name="id">accountId</param>
        /// <response code="204">Thành công</response>
        /// <response code="403">Trạng thái isDeleted bị trùng.</response>
        /// <response code="404">Không tìm thấy Tài Khoản</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public IActionResult Delete([Required, EmailAddress] string id)
        {
            return SetIsDeleted(id, true);
        }
                    
        /// <summary>
        /// Khôi phục Tài Khoản đã bị xóa.
        /// </summary>
        /// <param name="id">accountId</param>
        /// <response code="204">Thành công</response>
        /// <response code="403">Trạng thái isDeleted bị trùng.</response>
        /// <response code="404">Không tìm thấy Tài Khoản</response>   
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public IActionResult Patch([Required, EmailAddress] string id)
        {
            return SetIsDeleted(id, false);
        }

        private IActionResult SetIsDeleted(string id, bool isDelete)
        {
            var account = db.Accounts.Find(id.ToLower());

            if (account == null)
            {
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy tài khoản có id [{id}]"
                }.Result();
            }

            if (account.IsDeleted.Equals(isDelete))
            {
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = $"Tài khoản này {(isDelete ? "đã bị xóa trước đó" : "không trong trạng thái bị xóa")} nên không thể thao tác."
                }.Result();
            }

            account.IsDeleted = isDelete;

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

            return NoContent();
        }
    }
}