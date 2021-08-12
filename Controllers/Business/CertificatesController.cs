using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace Api.Controllers.Business
{
    [Route("api/business/[controller]")]
    [ApiController]
    [Authorize(Roles = "supplier")]
    public class CertificatesController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Trả về các chứng chỉ, giấy chứng nhận của Tài Khoản Bán Hàng.
        /// </summary>
        /// <response code="200">Thành công và trả về thông tin.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Certificate>), StatusCodes.Status200OK)]
        public IEnumerable<Certificate> Get()
        {
            return db.Certificates.Where(p => p.SupplierId.Equals(User.FindFirstValue(ClaimTypes.Sid).ToLower()));
        }

        /// <summary>
        /// Thêm chứng chỉ, giấy chứng nhận vào thông tin của Tài Khoản Bán Hàng.
        /// </summary>
        /// <param name="certificate">Thông tin chứng chỉ. Không cần truyền SupplierId. Mọi trường khác kiểu dữ liệu nguyên thủy và DateTime khi truyền vào đều không có giá trị (luôn bị bỏ qua).</param>
        /// <response code="201">Thành công và trả về thông tin vừa tạo.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Certificate), StatusCodes.Status201Created)]
        public IActionResult Post([Required] Certificate certificate)
        {
            certificate.SupplierId = User.FindFirstValue(ClaimTypes.Sid).ToLower();
            certificate.SetNullObjectChildren();

            try
            {
                db.Certificates.Add(certificate);
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

            return Created("https://bestsv.net", certificate);
        }

        /// <summary>
        /// Cập nhật thông tin.
        /// </summary>
        /// <param name="id">CertificateId</param>
        /// <param name="certificate">Thông tin cập nhật. Không cần truyền SupplierId. Mọi trường khác kiểu dữ liệu nguyên thủy và DateTime khi truyền vào đều không có giá trị (luôn bị bỏ qua).</param>
        /// <response code="204">Thành công.</response>
        /// <response code="403">Không được phép sửa tài nguyên của người khác.</response>
        /// <response code="404">Không tìm thấy.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public IActionResult Put([Required] int id, [Required] Certificate certificate)
        {
            var findId = db.Certificates.Find(id);

            if (findId == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy."
                }.Result();

            if (findId.SupplierId != User.FindFirstValue(ClaimTypes.Sid).ToLower())
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Không có quyền chỉnh sửa tài nguyên của người khác.",
                    SupportUrl = "https://api.bestsv.net/api/business/certificates",
                    RepairGuides = new string[]
                    {
                        "Kiểm ra kỹ lại CertificateId."
                    }
                }.Result();

            certificate.SetNullObjectChildren();

            try
            {
                db.Certificates.Update(certificate);
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

        /// <summary>
        /// Xóa chứng chỉ, giấy phép của Tài Khoản Bán Hàng theo <paramref name="id"/>
        /// </summary>
        /// <param name="id">CertificateId</param>
        /// <response code="204">Thành công.</response>
        /// <response code="403">Không được phép sửa tài nguyên của người khác.</response>
        /// <response code="404">Không tìm thấy.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public IActionResult Delete([Required] int id)
        {
            var certificate = db.Certificates.Find(id);

            if (certificate == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy."
                }.Result();

            if (certificate.SupplierId != User.FindFirstValue(ClaimTypes.Sid).ToLower())
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Không có quyền chỉnh sửa tài nguyên của người khác.",
                    SupportUrl = "https://api.bestsv.net/api/business/certificates",
                    RepairGuides = new string[]
                    {
                        "Kiểm ra kỹ lại CertificateId."
                    }
                }.Result();

            try
            {
                db.Certificates.Remove(certificate);
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