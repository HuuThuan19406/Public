using Api.Entities;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    /// <summary>
    /// Quản lý mã Token
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly BestsvContext db = new BestsvContext();

        private readonly string[] repairWrongPassword = new string[]
        {
            "Mật khẩu có phân biệt chữ in hoa và chữ thường, vui lòng kiểm tra kỹ lại.",
            "Nếu như quên mật khẩu, bạn không nên cố gắng thử lại. Hãy sử dụng tính năng Quên Mật Khẩu."
        };

        private readonly Authenticator authenticator = new Authenticator();

        /// <summary>
        /// Cấp mã Token để gọi các API cần xác thực.
        /// </summary>
        /// <param name="id">Tài khoản đăng nhập.</param>
        /// <param name="password">Mật khẩu đăng nhập.</param>
        /// <param name="needRefreshToken">Nếu nhận giá trị true nếu cần lấy RefreshToken. RefreshToken (thời hạn 90 ngày) để làm mới Token. Mỗi tài khoản chỉ có thể lưu tối đa 5 RefreshToken gần nhất (các <paramref name="needRefreshToken"/> cũ sẽ bị thu hồi).</param>
        /// <returns>Chuỗi kí tự là Bearer Token để xác thực.</returns>
        /// <response code="201">Thành công và trả về Token.</response>
        /// <response code="401">Mật khẩu không chính xác.</response>
        /// <response code="403">Tài khoản đã bị vô hiệu hóa.</response>
        /// <response code="404">Không tìm thấy tài khoản.</response>
        /// <response code="429">Nhập sai mật khẩu quá nhiều lần (nhiều hơn 3).</response>
        [HttpGet]
        [ProducesResponseType(typeof(Authenticator), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status429TooManyRequests)]
        public IActionResult Get([Required][EmailAddress] string id, [Required] string password, bool needRefreshToken = false)
        {
            IActionResult response;
            if (CheckHackingPassword(out response))
                return response;

            id = id.ToLower();

            switch (LoginHandler.IsValid(id, password, out Account account))
            {
                case null:
                    return new StatusError
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = $"Không tìm thấy tài khoản [{id}]"
                    }.Result();

                case false:
                    //Tránh thử mật khẩu liên tục
                    if (HttpContext.Session.Keys.Contains("TestPasswordTimes"))
                    {
                        HttpContext.Session.SetInt32("TestPasswordTimes", HttpContext.Session.GetInt32("TestPasswordTimes").Value + 1);

                        if (HttpContext.Session.GetInt32("TestPasswordTimes") > 2)
                        {
                            HttpContext.Session.Set<DateTime>
                                (
                                    "CanLoginAt",
                                    DateTime.UtcNow.AddMinutes(HttpContext.Session.GetInt32("TestPasswordTimes").Value * 1.5)
                                );
                        }
                    }
                    else
                        HttpContext.Session.SetInt32("TestPasswordTimes", 1);
                    if (CheckHackingPassword(out response))
                        return response;

                    return new StatusError
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = $"Mật khẩu không chính xác. {(HttpContext.Session.GetInt32("TestPasswordTimes") == 2 ? "Nếu nhập sai mật khẩu lần nữa bạn sẽ bị tạm khóa đăng nhập." : null)}",
                        RepairGuides = repairWrongPassword
                    }.Result();
            }

            if (account.IsDeleted)
            {
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ Hỗ Trợ để biết thêm chi tiết."
                }.Result();
            }

            new Task(() =>
            {
                HttpContext.Session.Remove("TestPasswordTimes");
                HttpContext.Session.Remove("CanLoginAt");
            }).Start();

            var roles = db.AccountRoles.Where(p => p.AccountId.Equals(id)).Select(p => p.Role.RoleName);
            authenticator.GenerateToken(id, roles, Request.Host.Value);

            if (needRefreshToken)
            {
                authenticator.GenerateRefreshToken((token) =>
                {
                    int countRefreshToken = db.RefreshTokens.Count(p => p.AccountId.Equals(id));
                    if (countRefreshToken >= 5)
                    {
                        var refreshTokenNeedDeleted = db
                            .RefreshTokens
                            .Where(p => p.AccountId.Equals(id))
                            .OrderBy(p => p.Expired)
                            .Take(countRefreshToken + 1 - 5)
                            .ToHashSet();

                        foreach (var item in refreshTokenNeedDeleted)
                        {
                            db.RefreshTokens.Remove(item);
                        }
                    }
                    db.RefreshTokens.Add(new RefreshToken
                    {
                        AccountId = id,
                        RefreshTokenId = Guid.Parse(token.Value),
                        Expired = token.Expired,
                        Ipaddress = HttpContext.Connection.RemoteIpAddress.ToString()
                    });

                    db.SaveChanges();
                });
            }

            db.RefreshTokens.RemoveRange(db.RefreshTokens.Where(p => p.AccountId.Equals(account.AccountId) && p.Expired < DateTime.UtcNow));

            account.LastLogin = DateTime.UtcNow;
            db.Accounts.Update(account);
            db.SaveChangesAsync().Wait();

            return Created("https://api.bestsv.net", authenticator);
        }

        /// <summary>
        /// Cấp phát mã Token mới dựa vào RefreshToken
        /// </summary>
        /// <param name="refreshTokenId">Mã RefreshToken được cấp</param>
        /// <response code="400"><paramref name="refreshTokenId"/> không đúng dịnh dạng Guid</response>
        /// <response code="403">Tài khoản đã bị vô hiệu hóa.</response>
        /// <response code="404">Không tìm thấy dữ liệu vể <paramref name="refreshTokenId"/>. Có thể mã đã bị thu hồi.</response>
        /// <response code="408"><paramref name="refreshTokenId"/> đã hết hạn.</response>
        [HttpGet("{refreshTokenId}")]
        [ProducesResponseType(typeof(Token), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusError), StatusCodes.Status408RequestTimeout)]
        public IActionResult Get([Required] string refreshTokenId)
        {
            if (Guid.TryParse(refreshTokenId, out Guid guid) == false)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = $"Refresh Token [{refreshTokenId}] không đúng định dạng."
                }.Result();

            var refreshToken = db.RefreshTokens.Find(guid);

            if (refreshToken == null)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy Refresh Token [{refreshTokenId}]."
                }.Result();

            if (refreshToken.Expired < DateTime.UtcNow)
                return new StatusError
                {
                    StatusCode = StatusCodes.Status408RequestTimeout,
                    Message = $"Mã đã hết hạn.",
                    RepairGuides = new string[]
                    {
                        "Gọi api [HttpGet] https://api.bestsv.net/token?id={tài khoản}&password={mật khẩu} để được cấp lại mã."
                    }
                }.Result();

            string id = refreshToken.AccountId;

            if (db.Accounts.Find(id).IsDeleted)
            {
                return new StatusError
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ Hỗ Trợ để biết thêm chi tiết."
                }.Result();
            }

            var roles = db.AccountRoles.Where(p => p.AccountId.Equals(id)).Select(p => p.Role.RoleName);
            authenticator.GenerateToken(id, roles, Request.Host.Value);

            var account = db.Accounts.Find(id);
            account.LastLogin = DateTime.UtcNow;
            db.Accounts.Update(account);
            db.SaveChangesAsync().Wait();

            return Created("https://api.bestsv.net", authenticator.Token);
        }

        /// <summary>
        /// Kiểm tra xem có dấu hiệu đang dò mật khẩu không
        /// </summary>
        private bool CheckHackingPassword(out IActionResult response)
        {
            if (HttpContext.Session.Keys.Contains("CanLoginAt") && (HttpContext.Session.Get<DateTime>("CanLoginAt") > DateTime.UtcNow))
            {
                var timeLeft = HttpContext.Session.Get<DateTime>("CanLoginAt") - DateTime.UtcNow;

                response = new StatusError
                {
                    StatusCode = StatusCodes.Status429TooManyRequests,
                    Message = $"Bạn đã thử xác thực sai quá nhiều lần. Hành vi này bị nghi ngờ là đang cố dò mật khẩu nên bạn sẽ không thể gửi yêu cầu cho đến {timeLeft.Minutes} phút {timeLeft.Seconds} giây sau.",
                    RepairGuides = repairWrongPassword
                }.Result();

                return true;
            }
            response = null;
            return false;
        }
    }
}