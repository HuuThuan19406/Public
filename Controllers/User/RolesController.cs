using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Api.Controllers.User
{
    /// <response code="401">Chưa xác thực hoặc xác thực thất bại</response>
    /// <response code="409">Xung đột dữ liệu</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/user/[controller]")]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Trả về danh sách Quyền Hạn
        /// </summary>
        /// <response code="200">Danh sách Quyền Hạn</response>
        [HttpGet]
        public IEnumerable<Role> Get()
        {
            return db
                   .AccountRoles
                   .Where(p => p
                        .AccountId
                        .Equals(User
                            .FindFirstValue(ClaimTypes.Sid)
                            .ToLower()))
                   .Select(p => p.Role);
        }
    }
}