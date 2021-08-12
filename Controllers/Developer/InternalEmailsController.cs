using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Developer
{
    /// <summary>
    /// Quản lý Email nội bộ BestSV
    /// </summary>
    /// <response code="401">Chưa xác thực hoặc xác thực thất bại</response>
    /// <response code="409">Xung đột dữ liệu</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/developer/[controller]")]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ApiController]
    [Authorize(Roles = "developer")]
    public class InternalEmailsController : ControllerBase
    {
        /// <summary>
        /// Gửi thông tin email nội bộ được cấp cho thành viên BestSV.
        /// </summary>
        /// <param name="sendTo">Địa chỉ email cá nhân người nhận.</param>
        /// <param name="email">Email nội bộ được cấp.</param>
        /// <param name="lastName">Tên người nhận.</param>
        /// <response code="204">Thành công.</response>
        [HttpPost("{sendTo}")]
        public IActionResult Post([Required] string sendTo, [Required] Email email, [Required] string lastName)
        {
            new DeveloperMailHelper().SendInformationInternalEmail(sendTo, email, lastName);
            return NoContent();
        }
    }
}
