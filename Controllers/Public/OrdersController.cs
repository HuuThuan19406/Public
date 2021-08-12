using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Api.Controllers.Public
{
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/public/[controller]")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Lấy ra những Đơn Đặt Hàng theo điều kiện lọc (nếu có).
        /// </summary>
        /// <param name="skip">Vị trí dòng bắt đầu lấy dữ liệu.</param>
        /// <param name="take">Số lượng dòng dữ liệu lấy ra kể từ dòng <paramref name="skip"/></param>
        /// <param name="categoryId">Đơn Đặt Hàng có chứa sản phẩm thuộc Thể Loại tương ứng.</param>
        /// <param name="tagId">Đơn Đặt Hàng được gắn Tag tương ứng. Tag không phân biệt hoa thường.</param>
        /// <param name="isPending">Chỉ hiện Đơn Hàng công khai chưa có người nhận nếu giá trị truyền vào là true. Mặc định là true.</param>
        /// <param name="isOptimizeSize">Mặc định khi thực hiện API này thì OrderDetails và OrderTags cũng được tải. Nếu isOptimizeSize mang giá trị true thì OrderDetails và OrderTags sẽ không được trả về chung với phản hồi. Kích thước của phản hồi sẽ được giảm nhưng thời gian nhận được phản hồi có thể tăng một phần không đáng kể. isOptimizeSize mặc định là true.</param>
        /// <response code="200">Thành công và trả về thông tin.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
        public IEnumerable<Order> Get([Required] int skip, [Required] int take, byte? categoryId, string tagId, bool isPending = true, bool isOptimizeSize = true)
        {
            var filterCategory = new Func<Order, bool>
                (
                    p => categoryId.HasValue
                        ? p.OrderDetails
                            .Any(o => o.CategoryId.Equals(categoryId.Value) || CategoryHandler.GetAllChildren(categoryId.Value).Contains(o.CategoryId))
                        : true
                );
            var filterTag = new Func<Order, bool>(p => string.IsNullOrEmpty(tagId) ? true : p.OrderTags.Any(p => p.TagId.Equals(tagId.ToLower())));

            var data = db
                .Orders                
                .Where(p => isPending ? p.ProcessStatusId.Equals(1) : ((p.ProcessStatusId < 8) && !p.IsDeleted))
                .Include(p => p.OrderTags)
                .Include(p => p.OrderDetails)
                .Where(filterTag)
                .Where(filterCategory)
                .Skip(skip)
                .Take(take);

            TaskArray taskArray = new TaskArray(0);

            if (isOptimizeSize)
            {
                taskArray = new TaskArray(data.Count());

                foreach (var item in data)
                {
                    taskArray.AddAndStart(() => item.SetNullObjectChildren());
                }
            }

            foreach (var item in data)
            {
                item.SetNullProperties("DescriptionFileUri");
            }

            taskArray.WaitAll();

            return data;
        }

        /// <summary>
        /// Trả về thông tin của Đơn Đặt Hàng theo <paramref name="id"/>, bao gồm chi tiết các dòng đặt hàng.
        /// </summary>
        /// <param name="id">OrderId</param>
        /// <response code="200">Thành công và trả về thông tin.</response>
        /// <response code="404">Không tìm thấy.</response>
        /// <response code="423">Đơn bị lỗi hoặc đã thu hồi nên không thể xem.</response> 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status423Locked)]
        public IActionResult Get([Required] int id)
        {
            var order = db.Orders.Find(id);

            if (order == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy"
                }.Result();

            if (order.IsDeleted)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn đã bị xóa, không thể thao tác."
                }.Result();

            if (order.ProcessStatusId.Equals(8))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status423Locked,
                    Message = "Đơn bị lỗi nên không thể xem."
                }.Result();

            var orderDetails = db.OrderDetails.Where(p => p.OrderId.Equals(id)).ToList();
            order.OrderDetails = orderDetails;

            order.SetNullProperties("DescriptionFileUri");
            foreach (var item in order.OrderDetails)
            {
                item.SetNullProperties("FileUri");
            }

            return Ok(order);
        }

        /// <summary>
        /// Trả về số lượng Đơn Hàng của Người Mua hoặc Người Bán tùy theo <paramref name="type"/>
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="type">0: Người Mua, 1: Người Bán</param>
        /// <response code="200">Trả về số lượng Đơn Hàng.</response>
        /// <response code="400">Type không hợp lệ.</response>
        /// <response code="404">Không tìm thấy Người Mua hoặc Người Bán.</response>
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        [HttpGet("count")]
        public IActionResult Get([Required, EmailAddress] string accountId, [Required, Range(0,1)] byte type)
        {
            accountId = accountId.ToLower();

            switch (type)
            {
                case 0:
                    var buyer = db.Accounts.Find(accountId);
                    if (buyer == null)
                        return new StatusError
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = $"Người mua [{accountId}] không tồn tại."
                        }.Result();
                    return Ok(db.Orders.Count(p => p.AccountId.Equals(accountId)));
                case 1:
                    var supplier = db.Suppliers.Find(accountId);
                    if (supplier == null)
                        return new StatusError
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = $"Người bán [{accountId}] không tồn tại."
                        }.Result();
                    return Ok(db.Orders.Count(p => p.SupplierId.Equals(accountId)));
            }

            return new StatusError
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = $"Type [{type}] không hợp lệ."
            }.Result();
        }
    }
}