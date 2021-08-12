using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers.Public
{
    /// <response code = "401" > Chưa xác thực hoặc xác thực thất bại</response>
    /// <response code="409">Xung đột dữ liệu</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/public/[controller]")]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Trả thông tin công khai về Tài Khoản theo <paramref name="id"/>.
        /// </summary>
        /// <param name="id">AccountId</param>
        /// <response code="200">Thành công và trả về thông tin Tài Khoản.</response>
        [HttpGet("{id}")]
        public IActionResult Get([Required, EmailAddress] string id)
        {
            var account = db.Accounts.Find(id);

            if (account == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy."
                }.Result();

            return Ok(new Account
            {
                AccountId = account.AccountId,
                UserName = account.UserName,
                CreatedAt = account.CreatedAt
            });
        }

        /// <summary>
        /// Tạo Tài Khoản mới
        /// </summary>
        /// <param name="account">
        ///     Thông tin Tài Khoản cần khởi tạo. Không cần truyền CreateAt, LastLogin, IsDeleted và SecretKey.
        ///     SerectKey là chuỗi ký tự ngẫu nhiên gồm 6 kí tự để Người Dùng xác thực toàn vẹn thông tin nhạy cảm gửi/nhận. Mã chỉ hiển thị một lần duy nhất khi cấp mã, Người Dùng cần lưu giữ và bảo mật mã này. Có thể xin cấp lại SerectKey không giới hạn.
        ///     Mọi trường khác kiểu dữ liệu nguyên thủy và DateTime khi truyền vào đều không có giá trị (luôn bị bỏ qua).
        /// </param>
        /// <param name="PIN">Mã xác minh gồm 6 chữ số. Lấy mã pin tại https://api.bestsv.net/api/public/pin/{id}</param>
        /// <response code="201">Trả về thông tin Tài Khoản.</response>
        /// <response code="401 ">Sai mã <paramref name="PIN"/>.</response>
        /// <response code="404">Không tìm thấy mã <paramref name="PIN"/>.</response>
        /// <response code="406">Mã <paramref name="PIN"/> hết thời hạn hiệu lực.</response>
        /// <response code="412">Id đăng ký Tài Khoản đã tồn tại.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Account), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status412PreconditionFailed)]
        public async Task<IActionResult> PostAsync([Required] Account account, [Required][MinLength(6)][MaxLength(6)] string PIN)
        {
            account.AccountId = account.AccountId.ToLower();

            TaskArray taskArray = new TaskArray(3);
            taskArray.AddAndStart(() => account.SetNullObjectChildren());
            taskArray.AddAndStart(() => account.Password = new Cipher(account.AccountId).GenerateSignature(account.Password));
            taskArray.AddAndStart(() =>
            {
                account.LastLogin = account.CreatedAt = DateTime.UtcNow;
                account.IsDeleted = false;
                account.SecrectKey = AlphaNumberStringRandom.Generate(6);
            });

            if (db.Accounts.Any(p => p.AccountId.Equals(account.AccountId)))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status412PreconditionFailed,
                    Message = $"Tài khoản với id [{account.AccountId}] đã tồn tại.",
                    RepairGuides = new string[]
                    {
                        "Vui lòng chọn một id khác. Lưu ý, id không phân biệt chữ in hoa và chữ thường."
                    }
                }.Result();

            taskArray.WaitAll();

            var addAccount = db.Accounts.AddAsync(account);

            var responseCheckPIN = PINHandler.ResponseCheckPIN(account.AccountId, PIN);
            if (responseCheckPIN != null)
                return responseCheckPIN;

            try
            {
                await addAccount;
                await db.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                return new StatusError
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = ex.Message
                }.Result();
            }

            account.SetNullProperties("Password");

            return Created("https://bestsv.net", account);
        }

        /// <summary>
        /// Đổi mật khẩu của Tài Khoản theo <paramref name="id"/>
        /// </summary>
        /// <param name="id">id của Tài Khoản cần đổi</param>
        /// <param name="newPassword">Mật khẩu mới</param>
        /// <param name="PIN">Mã xác minh gồm 6 chữ số. Lấy mã pin tại https://api.bestsv.net/api/public/pin/{id}</param>
        /// <response code="204">Thành công</response>
        /// <response code="401">Sai mã <paramref name="PIN"/></response>
        /// <response code="404">Không tìm thấy mã <paramref name="PIN"/> hoặc Tài Khoản không tồn tại</response>
        /// <response code="406">Mã <paramref name="PIN"/> hết thời hạn hiệu lực</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status406NotAcceptable)]
        public IActionResult ChangePassword([Required, EmailAddress] string id, [Required] string newPassword, [Required, MaxLength(6), MinLength(6)] string PIN)
        {
            id = id.ToLower();
            IActionResult responseCheckPIN = null;
            Account account = null;

            TaskArray taskArray = new TaskArray(2);
            taskArray.AddAndStart(() => responseCheckPIN = PINHandler.ResponseCheckPIN(id, PIN));
            taskArray.AddAndStart(() => account = db.Accounts.Find(id));

            taskArray.WaitAll();

            if (responseCheckPIN != null)
                return responseCheckPIN;

            if (account == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy tài khoản có id [{id}]"
                }.Result();

            account.Password = new Cipher(id).GenerateSignature(newPassword);

            try
            {
                db.Accounts.Update(account);
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