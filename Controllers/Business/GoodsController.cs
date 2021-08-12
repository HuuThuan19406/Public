using Api.Entities;
using Api.Models;
using GoogleApi.Drive;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace Api.Controllers.Business
{
    /// <summary>Quản lý những Sản Phẩm của Người Bán.</summary>
    /// <response code="401">Chưa xác thực hoặc xác thực thất bại</response>
    /// <response code="409">Xung đột dữ liệu</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/business/[controller]")]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ApiController]
    [Authorize(Roles = "supplier")]
    public class GoodsController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Lấy danh sách Sản Phẩm đã tải lên của Người Bán theo điều kiện lọc (nếu có).
        /// </summary>
        /// <param name="skip">Vị trí dòng bắt đầu lấy dữ liệu.</param>
        /// <param name="take">Số lượng dòng dữ liệu lấy ra kể từ dòng <paramref name="skip"/></param>
        /// <param name="fromDay">Điều kiện ngày tải Sản Phẩm UploadDate từ thời gian này đến hiện tại.</param>
        /// <param name="toDay">Điều kiện ngày tài Sản Phẩm UploadDate từ thời gian này trở về trước.</param>
        /// <response code="200">Thành công và trả về thông tin.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Good>), StatusCodes.Status200OK)]
        public IEnumerable<Good> Get([Required] int skip, [Required] int take, DateTime? fromDay, DateTime? toDay)
        {
            var filterFromDay = new Func<Good, bool>(p => fromDay.HasValue ? p.UploadDate.AddMinutes(7) >= fromDay.Value : true);
            var filterToDay = new Func<Good, bool>(p => fromDay.HasValue ? p.UploadDate.AddMinutes(7) <= toDay.Value : true);

            return db
                .Goods
                .Where
                (
                    p => p
                    .SupplierId
                    .Equals(User.FindFirstValue(ClaimTypes.Sid).ToLower())
                )
                .Where(filterFromDay)
                .Where(filterToDay)
                .Skip(skip)
                .Take(take);
        }

        /// <summary>
        /// Đăng Sản Phẩm cá nhân để rao bán.
        /// </summary>
        /// <param name="goodsUploadHandler">Thông tin Sản Phẩm cần rao bán. Có thể không cần nhập Note.</param>
        /// <response code="201">Thành công.</response>
        /// <response code="403">Category của OrderDetail phải là nguyên tử, nghĩa là không thể chi tiết hơn nữa.</response>
        [HttpPost]
        public IActionResult Post([Required] GoodsUploadHandler goodsUploadHandler)
        {
            Good goods = new Good
            {
                SupplierId = User.FindFirstValue(ClaimTypes.Sid).ToLower(),
                CategoryId = goodsUploadHandler.CategoryId,
                ProductName = goodsUploadHandler.ProductName,
                Price = goodsUploadHandler.Price,
                Note = goodsUploadHandler.Note,
                UploadDate = DateTime.UtcNow
            };

            if (db.Categories.Any(p => p.ParentCategoryId.Value.Equals(goods.CategoryId)))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Sản phẩm yêu cầu phải có Thể Loại là bậc dưới cùng, nghĩa là không thể chi tiết hơn được nữa."
                }.Result();

            var cloud = new GoogleDriveApi();
            var file = new GoogleDriveFileCreated(goodsUploadHandler.FileName, goodsUploadHandler.DataFileBase64, db.Suppliers.Find(goods.SupplierId).FolderUri);

            goods.GoodsId = cloud.UploadFile(file);

            cloud = null;
            file = null;

            try
            {
                db.Goods.Add(goods);
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

            goods.SetNullProperties("GoodsId");
            goods.SetNullObjectChildren();

            return Created("https://bestsv.net", goods);
        }
    }
}