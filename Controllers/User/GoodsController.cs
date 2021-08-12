using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace Api.Controllers.User
{
    /// <summary>
    /// Quản lý những Sản Phẩm đã mua - dành cho Người Dùng
    /// </summary>
    /// <response code="401">Chưa xác thực hoặc xác thực thất bại</response>
    /// <response code="409">Xung đột dữ liệu</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/user/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status409Conflict)]
    [Authorize]
    public class GoodsController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Lấy danh sách Sản Phẩm đã mua theo điều kiện lọc (nếu có).
        /// </summary>
        /// <param name="skip">Vị trí dòng bắt đầu lấy dữ liệu.</param>
        /// <param name="take">Số lượng dòng dữ liệu lấy ra kể từ dòng <paramref name="skip"/></param>
        /// <param name="categoryId">Sản Phẩm thuộc thể loại CategoryId tương ứng.</param>
        /// <param name="fromDay">Điều kiện ngày tải Sản Phẩm UploadDate từ thời gian này đến hiện tại.</param>
        /// <param name="toDay">Điều kiện ngày tài Sản Phẩm UploadDate từ thời gian này trở về trước.</param>
        /// <response code="200">Thành công và trả về thông tin.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Good>), StatusCodes.Status200OK)]
        public IEnumerable<Good> Get([Required] int skip, [Required] int take, byte? categoryId, DateTime? fromDay, DateTime? toDay)
        {
            var filterCategory = new Func<Good, bool>
                (
                    p => categoryId.HasValue
                        ? p.CategoryId.Equals(categoryId.Value) || CategoryHandler.GetAllChildren(categoryId.Value).Contains(p.CategoryId)
                        : true
                );
            var filterFromDay = new Func<Good, bool>(p => fromDay.HasValue ? p.UploadDate.AddMinutes(7) >= fromDay.Value : true);
            var filterToDay = new Func<Good, bool>(p => fromDay.HasValue ? p.UploadDate.AddMinutes(7) <= toDay.Value : true);

            var data = db
                .Purchases
                .Where(p => p.AccountId.Equals(User.FindFirstValue(ClaimTypes.Sid).ToLower()))
                .Select(p => p.Goods)
                .Where(filterCategory)
                .Where(filterFromDay)
                .Where(filterToDay)
                .Skip(skip)
                .Take(take);

            foreach (var item in data)
            {
                item.SetNullProperties("GoodsId");
            }

            return data;
        }
    }
}