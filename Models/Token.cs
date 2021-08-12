using System;

namespace Api.Models
{
    /// <summary>
    /// Mã xác thực danh tính
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Giá trị chuỗi của Token
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Thời gian hết hạn của Token
        /// </summary>
        public DateTime Expired { get; set; }
    }
}