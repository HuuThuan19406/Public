using Api.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Api.Models
{
    public static class PINHandler
    {
        public static TimeSpan DURATION = TimeSpan.FromMinutes(15);

        /// <summary>
        /// Kiểm tra trạng thái mã PIN
        /// </summary>
        /// <param name="id">id cần đối chiếu <paramref name="PIN"/></param>
        /// <param name="PIN">Mã do người dùng truyền để xác thực danh tính của họ</param>
        /// <returns>0: Mã chính xác<br/>1: Không tìm thấy id yêu cầu <paramref name="PIN"/><br/>2: Mã không chính xác<br/>3: Mã hết hạn hiệu lực<br/></returns>
        public static byte CheckPIN(string id, string PIN)
        {
            using (var db = new BestsvContext())
            {
                var identification = db.Identifications.FindAsync(id).Result;

                if (identification == null)
                    return 1;

                if (!new Cipher((identification.Expired.Add(-DURATION).Ticks / 1000000).ToString()).GenerateSignature(PIN)
                    .Equals(identification.Pin))
                    return 2;

                if (identification.Expired < DateTime.UtcNow)
                    return 3;
            }
            return 0;
        }

        public static IActionResult ResponseCheckPIN(string id, string PIN)
        {
            switch (PINHandler.CheckPIN(id, PIN))
            {
                case 1:
                    return new StatusError
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = $"Không tìm thấy mã PIN cho id [{id}]."
                    }.Result();

                case 2:
                    return new StatusError
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = $"Sai mã PIN [{PIN}]."
                    }.Result();

                case 3:
                    return new StatusError
                    {
                        StatusCode = StatusCodes.Status408RequestTimeout,
                        Message = $"Mã PIN đã hết thời gian hiệu lực.",
                        SupportUrl = $"https://api.bestsv.net/api/public/pin/{id}",
                        RepairGuides = new string[]
                        {
                            $"Vui lòng yêu cầu mã PIN mới tại [HttpGet] https://api.bestsv.net/api/public/pin/{id}"
                        }
                    }.Result();
            }

            return null;
        }
    }
}