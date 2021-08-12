using Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
    interface IOrderAutoMail
    {
        /// <summary>
        /// Thông báo Đơn Hàng đã có người nhận.
        /// </summary>
        bool NotifyOrderReceived(Order order);

        /// <summary>
        /// Thông báo Đơn Hàng có Thương Lượng mới.
        /// </summary>
        bool NotifyOrderHasNewNegotiate(Negotiation negotiate);

        /// <summary>
        /// Thông báo Chi Tiết Đơn Hàng có sản phẩm mới tải lên, chờ duyệt.
        /// </summary>
        bool NotifyOrderDetailWasUpdated(OrderDetail orderDetail);

        /// <summary>
        /// Thông báo Đơn Hàng hoàn tất.
        /// </summary>
        bool NotifySuccessOder(Order order);
    }
}
