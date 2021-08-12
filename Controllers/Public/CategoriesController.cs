using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Api.Controllers.Public
{
    [Route("api/public/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Trả về danh sách Thể Loại dưới dạng sơ đồ cây.
        /// </summary>
        /// <response code="200">Trả về thông tin.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Category>), StatusCodes.Status200OK)]
        public IEnumerable<Category> Get()
        {
            var categories = db
                            .Categories
                            .Include(p => p.InverseParentCategory)
                            .Select
                            (
                                p => new Category
                                {
                                    CategoryId = p.CategoryId,
                                    CategoryName = p.CategoryName,
                                    Description = p.Description,
                                    ParentCategoryId = p.ParentCategoryId,
                                    InverseParentCategory = p.InverseParentCategory
                                }
                            )
                            .ToList();

            for (int i = categories.Count - 1; i >= 0; i--)
            {
                if (categories[i].ParentCategoryId != null)
                    categories.RemoveAt(i);
            }

            return categories;
        }

        /// <summary>
        /// Trả về thông tin Thể Loại theo <paramref name="id"/>
        /// </summary>
        /// <param name="id">CategoryId</param>
        /// <response code="200">Trả về thông tin.</response>
        /// <response code="404">Không tìm thấy.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        public IActionResult Get(byte id)
        {
            var category = db.Categories.Find(id);

            if (category != null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy."
                }.Result();

            return Ok(category);
        }
    }
}