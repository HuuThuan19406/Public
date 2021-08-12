using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Api.Controllers.Public
{
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/public/[controller]")]
    [ApiController]
    public class AvatarsController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Lấy danh sách Avatar.
        /// </summary>
        /// <response code="200">Thành công và trả về thông tin.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AvatarBase64>), StatusCodes.Status200OK)]
        public IEnumerable<AvatarBase64> Get()
        {
            return db.Avatars.Select(p => new AvatarBase64(p));
        }

        /// <summary>
        /// Lấy thông tin Avatar theo <paramref name="id"/>.
        /// </summary>
        /// <param name="id">AvatarId</param>
        /// <response code="200">Thành công và trả về thông tin.</response>
        /// <response code="404">Không tìm thấy.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AvatarBase64), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status200OK)]
        public IActionResult Get(byte id)
        {
            var avatar = db.Avatars.Find(id);

            if (avatar == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy."
                }.Result();

            return Ok(new AvatarBase64(avatar));
        }
    }
}