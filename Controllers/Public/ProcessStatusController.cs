using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Api.Controllers.Public
{
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/public/[controller]")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ApiController]
    public class ProcessStatusController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Lấy danh sách Trạng Thái Xử Lý.
        /// </summary>
        /// <response code="200">Thành công và trả về thông tin.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProcessStatus>), StatusCodes.Status200OK)]
        public IEnumerable<ProcessStatus> Get()
        {
            return db.ProcessStatuses;
        }

        /// <summary>
        /// Lấy thông tin Trạng Thái Xử Lý theo <paramref name="id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Thành công và trả về thông tin.</response>
        /// <response code="404">Không tìm thấy.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProcessStatus), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public IActionResult Get(byte id)
        {
            var processStatus = db.ProcessStatuses.Find(id);

            if (processStatus == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy."
                }.Result();

            return Ok(processStatus);
        }
    }
}