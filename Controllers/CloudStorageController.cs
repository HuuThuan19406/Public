using GoogleApi.Drive;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers
{
    /// <summary>
    /// Thao tác với Đám Mây Lưu Trữ Dữ Liệu.
    /// </summary>
    /// <response code="401">Chưa xác thực hoặc xác thực thất bại</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(500)]
    [Authorize(Roles = "storage_manager")]
    public class CloudStorageController : ControllerBase
    {
        private readonly GoogleDriveApi driveApi = new GoogleDriveApi();

        /// <summary>
        /// Lấy ra thông tin cơ bản của các thư mục và tệp trên Cloud
        /// </summary>
        [HttpGet]
        public IEnumerable<GoogleDriveFileInfoBase> Get()
        {
            return driveApi.GetDriveFiles();
        }

        /// <summary>
        /// Tải xuống tệp từ Cloud
        /// </summary>
        /// <param name="id">fileId</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public GoogleDriveFileCreated Get(string id)
        {
            return driveApi.DownloadFile(id);
        }

        /// <summary>
        /// Di chuyển thư mục hoặc tệp
        /// </summary>
        /// <param name="fileId">Id thư mục hoặc tệp</param>
        /// <param name="newParentId">Id thư mục cha nơi chuyển đến</param>
        /// <param name="oldParentId">Id của thư mục cha hiện tại. Không biết thì bỏ trống</param>
        [HttpPatch("{fileId}")]
        public void Move([Required] string fileId, [Required] string newParentId, string oldParentId)
        {
            if (string.IsNullOrEmpty(oldParentId))
                driveApi.MoveFileOrFolder(fileId, newParentId);
            else
                driveApi.MoveFileOrFolder(fileId, newParentId, oldParentId);
        }

        /// <summary>
        /// Tạo thư mục
        /// </summary>
        /// <param name="folderName">Tên thư mục</param>
        /// <param name="folderParentId">Id của thư mục cha (nếu có).</param>
        /// <response code="201">Tạo thành công và trả về id của thư mục</response>
        [HttpPost("{folderName}")]
        [ProducesResponseType(typeof(string), 201)]
        public IActionResult PostFolder([Required] string folderName, string folderParentId)
        {
            string id = driveApi.CreateFolder(folderName, folderParentId);
            return StatusCode(201, id);
        }

        /// <summary>
        /// Tải tệp lên
        /// </summary>
        /// <response code="201">Tạo thành công và trả về id của tệp</response>
        [HttpPost]
        [ProducesResponseType(typeof(string), 201)]
        public IActionResult PostFolder([Required] GoogleDriveFileCreated fileCreated)
        {
            string id = driveApi.UploadFile(fileCreated);
            return StatusCode(201, id);
        }
    }
}