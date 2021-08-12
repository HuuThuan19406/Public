using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Api.Controllers.Public
{
    [Route("api/public/[controller]")]
    [ApiController]
    public class CertificatesController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Trả về thông tin chứng chỉ, giấy chứng nhận của theo <paramref name="id"/>. Ai cũng có thể xem được.
        /// </summary>
        /// <param name="id">CertificateId</param>
        /// <response code="200">Thành công và trả về thông tin.</response>
        /// <response code="404">Không tìm thấy.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Supplier), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public IActionResult Get([Required] int id)
        {
            var certificate = db.Certificates.Find(id);

            if (certificate == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy."
                }.Result();

            return Ok(certificate);
        }

        /// <summary>
        /// Trả về thông tin chứng chỉ, giấy chứng nhận của theo <paramref name="supplierId"/>. Ai cũng có thể xem được.
        /// </summary>
        /// <response code="200">Thành công và trả về thông tin.</response>
        [HttpGet()]
        [ProducesResponseType(typeof(IEnumerable<Supplier>), StatusCodes.Status200OK)]
        public IEnumerable<Certificate> Get([Required, EmailAddress] string supplierId)
        {
            return db.Certificates.Where(p => p.SupplierId.Equals(supplierId));
        }
    }
}