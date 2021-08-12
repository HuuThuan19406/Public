using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Public
{
    /// <summary>
    /// Quản lý mã PIN
    /// </summary>
    /// <response code="409">Xung đột dữ liệu</response>
    /// <response code="500">Lỗi bên thứ 3 hoặc ngoại lệ chưa xác định</response>
    [Route("api/public/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(StatusError), StatusCodes.Status409Conflict)]
    public class PinController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        /// <summary>
        /// Gửi mã pin gồm 6 chữ số đến <paramref name="email"/>. Sau khi yêu cầu thành công cần 15 phút để xin cấp lại mã mới.
        /// </summary>
        /// <param name="email">Nếu đã có Tài Khoản chính là trường id</param>
        /// <response code="202">Yêu cầu đã được chấp nhận nhưng không có nghĩa email đã gửi thành công (sai email hoặc các lý do khác).</response>
        /// <response code="404">Không tìm thấy địa chỉ <paramref name="email"/>. Gửi thất bại.</response>
        /// <response code="429">Yêu cầu mã PIN nhiều lần trong thời gian ngắn.</response>
        [HttpGet("{email}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status429TooManyRequests)]
        public IActionResult Send([Required][EmailAddress] string email)
        {
            email = email.ToLower();
            var ident = db.Identifications.Find(email);

            if ((ident != null) && (ident.Expired > DateTime.UtcNow))
                return new StatusError
                {
                    StatusCode = StatusCodes.Status429TooManyRequests,
                    Message = $"Bạn đã yêu cầu cấp mã PIN cách đây không lâu. Vui lòng đợi đến {ident.Expired.AddHours(7).ToString("HH:mm:ss dd/MM/yy")} để xin cấp lại."
                }.Result();

            string PIN = new Random().Next(1000000).ToString("000000");
            DateTime UTCNow = DateTime.UtcNow;

            try
            {
                if (ident != null)
                {
                    ident.Pin = new Cipher((UTCNow.Ticks / 1000000).ToString()).GenerateSignature(PIN);
                    ident.Expired = UTCNow.Add(PINHandler.DURATION);
                    db.Identifications.Update(ident);
                }
                else
                {
                    ident = new Identification
                    {
                        IdentificationId = email,
                        Pin = new Cipher((UTCNow.Ticks / 1000000).ToString()).GenerateSignature(PIN),
                        Expired = UTCNow.Add(PINHandler.DURATION)
                    };
                    db.Identifications.Add(ident);
                }

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

            ident.Pin = PIN;

            if (new NoreplyFirstMailHelper().SendPin(ident) == false)
            {
                db.Identifications.Remove(db.Identifications.Find(ident.IdentificationId));

                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy địa chỉ email. Gửi thất bại."
                }.Result();
            }

            return Accepted();
        }
    }
}