using Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
    interface IAccountAutoMail
    {
        /// <summary>
        /// Gửi mã Pin dùng cho các thao tác yêu cầu xác thực.
        /// </summary>
        bool SendPin(Identification identification);

        /// <summary>
        /// Thông báo tạo Tài Khoản thành công.
        /// </summary>
        bool NotifyAccountGenerated(Account account);
    }
}
