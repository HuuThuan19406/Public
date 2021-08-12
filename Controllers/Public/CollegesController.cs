using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Public
{
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/public/[controller]")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ApiController]
    public class CollegesController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Lấy ra danh sách trường Đại học, Cao đẳng
        /// </summary>
        /// <response code="200">Thành công và trả về thông tin.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<College>), StatusCodes.Status200OK)]
        public IEnumerable<College> Get()
        {
            return db.Colleges;
        }

        /// <summary>
        /// Lấy ra thông tin trường Đại học, Cao đẳng theo <paramref name="id"/>.
        /// </summary>
        /// <param name="id">CollegeId.</param>
        /// <response code="200">Thành công và trả về thông tin.</response>
        /// <response code="404">Không tìm thấy.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ZipCode), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public IActionResult Get([Required] short id)
        {
            var college = db.Colleges.Find(id);

            if (college == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy."
                }.Result();

            return Ok(college);
        }
    }
}