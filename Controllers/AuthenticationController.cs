using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Api dùng cho kiểm thử việc xác thực
    /// </summary>
    /// <response code="401">Chưa xác thực hoặc xác thực thất bại</response>
    [Route("test/[controller]")]
    [ProducesResponseType(401)]
    [Authorize]
    public class AuthenticationController : Controller
    {
        /// <summary>
        /// Kiểm tra xem Token của bạn đã xác thực thành công chưa
        /// </summary>
        /// <response code="200">Thành công</response>
        /// <response code="401">Thất bại</response>
        [ProducesResponseType(typeof(string), 200)]
        [HttpPost]
        public IActionResult Login()
        {
            return Ok($"Chúc mừng tài khoản [{User.FindFirstValue(ClaimTypes.Sid).ToLower()}] xác thực Bearer Token thành công.");
        }

        /// <summary>
        /// Xác nhận Token có <paramref name="role"/> hay không
        /// </summary>
        /// <param name="role">Vai trò của người dùng để truy cập một số API nhất định</param>
        /// <response code="200">true: Người dùng có vai trò <paramref name="role"/>. false: Người dùng không có vai trò <paramref name="role"/></response>
        [Authorize]
        [ProducesResponseType(typeof(bool), 200)]
        [HttpGet]
        public bool IsInRole(string role)
        {
            return User.IsInRole(role);
        }
    }
}