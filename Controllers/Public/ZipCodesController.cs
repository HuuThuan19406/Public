using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Api.Controllers.Public
{
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/public/[controller]")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ApiController]
    public class ZipCodesController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Lấy ra danh sách 63 tỉnh thành
        /// </summary>
        /// <param name="isSortName">Nếu gán true thì danh sách trả về sẽ được sắp xếp tăng dần theo bảng chữ cái</param>
        /// <response code="200">Thành công và trả về thông tin</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ZipCode>), StatusCodes.Status200OK)]
        public IEnumerable<ZipCode> Get(bool isSortName)
        {
            if (isSortName)
                return db.ZipCodes.OrderBy(p => p.Position);

            return db.ZipCodes;
        }

        /// <summary>
        /// Lấy ra thông tin vị trí dựa theo <paramref name="id"/>
        /// </summary>
        /// <param name="id">Số gồm 6 chữ số là ZipCode</param>
        /// <response code="200">Thành công</response>
        /// <response code="404">Không tìm thấy</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ZipCode), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public IActionResult Get([Required] int id)
        {
            var zipCode = db.ZipCodes.Find(id);

            if (zipCode == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy."
                }.Result();

            return Ok(zipCode);
        }
    }
}