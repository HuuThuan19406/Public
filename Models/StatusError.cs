using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    /// <summary>
    /// Thông báo lỗi HTTP khi thực hiện truy vấn API
    /// </summary>
    public class StatusError
    {
        /// <summary>
        /// Mã trạng thái HTTP lỗi
        /// </summary>
        [Required]
        [Range(400, 599)]
        public int StatusCode { get; set; }

        /// <summary>
        /// Văn bản thông báo chi tiết về lỗi
        /// </summary>
        [Required]
        public string Message { get; set; }

        /// <summary>
        /// Đường dẫn có thể giúp giải quyết được vấn đề
        /// </summary>
        [Url]
        public string SupportUrl { get; set; }

        /// <summary>
        /// Những hướng dẫn giúp chuẩn đoán chính xác hơn sự cố hoặc có thể khắc phục lỗi
        /// </summary>
        [Required]
        public IEnumerable<string> RepairGuides { get; set; }

        public IActionResult Result()
        {
            SupportUrl = string.IsNullOrEmpty(SupportUrl) ? $"https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/{StatusCode}" : SupportUrl;
            return new ObjectResult(this)
            {
                StatusCode = this.StatusCode
            };
        }
    }
}