using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Api.Controllers.Business
{
    /// <summary>
    /// Nghiệp vụ liên quan đến Người Bán.
    /// </summary>
    /// <response code="401">Chưa xác thực hoặc xác thực thất bại</response>
    /// <response code="409">Xung đột dữ liệu</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/business/[controller]")]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ApiController]
    [Authorize(Roles = "supplier")]
    public class SuppliersController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Lấy ra thông tin Tài Khoản Bán Hàng
        /// </summary>
        /// <response code="200">Thành công và trả về thông tin</response>
        [HttpGet]
        [ProducesResponseType(typeof(Supplier), StatusCodes.Status200OK)]
        public Supplier Get()
        {
            return db.Suppliers.Find(User.FindFirstValue(ClaimTypes.Sid).ToLower());
        }

        /// <summary>
        /// Cập nhật thông tin Tài Khoản Bán Hàng.
        /// </summary>
        /// <param name="supplier">Thông tin cập nhật. Không cần truyền SupplierId. Mọi trường khác kiểu dữ liệu nguyên thủy và DateTime khi truyền vào đều không có giá trị (luôn bị bỏ qua).</param>
        /// <response code="204">Thành công</response>
        [HttpPut]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public IActionResult Put([Required] Supplier supplier)
        {
            supplier.SupplierId = User.FindFirstValue(ClaimTypes.Sid).ToLower();

            supplier.SetNullObjectChildren();

            try
            {
                db.Suppliers.Update(supplier);
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