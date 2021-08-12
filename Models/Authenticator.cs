using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Api.Models
{
    public class Authenticator
    {
        internal static byte[] SECRET { get; } = Encoding.UTF8.GetBytes("u9Xs7eLv6-=Bvmd@YN~w6}kCC04nux)om9ZHKUNAQ?*uPA%qh~r@=7Ds2-TEx8cds>oxQ9jT-kf)Vi*0!yXV7F^T@*bPiL.nDW9M");
        private static TimeSpan TOKEN_EXPIRE { get; } = TimeSpan.FromMinutes(5);
        private static TimeSpan REFRESH_TOKEN_EXPIRE { get; } = TimeSpan.FromDays(90);

        /// <summary>
        /// Mã xác thực danh tính để truy cập api
        /// </summary>
        public Token Token { get; set; }

        /// <summary>
        /// Mã xác thực dùng để xin cấp lại Token mà không cần đăng nhập
        /// </summary>
        public Token RefreshToken { get; set; }

        public void GenerateToken(string id, IEnumerable<string> roles, string host)
        {
            var claims = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Sid, id)
                });
            claims.AddClaims(roles.Select(p => new Claim(ClaimTypes.Role, p)));

            var tokenHandler = new JwtSecurityTokenHandler();
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.Add(TOKEN_EXPIRE),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(SECRET), SecurityAlgorithms.HmacSha256Signature)
            };
            descriptor.Issuer = descriptor.Audience = host;

            Token = new Token
            {
                Value = tokenHandler.WriteToken(tokenHandler.CreateToken(descriptor)),
                Expired = descriptor.Expires.Value
            };
        }

        public void GenerateRefreshToken(Action<Token> addonRefreshTokenToDatabase)
        {
            RefreshToken = new Token
            {
                Value = Guid.NewGuid().ToString(),
                Expired = DateTime.UtcNow.Add(REFRESH_TOKEN_EXPIRE)
            };

            addonRefreshTokenToDatabase(RefreshToken);
        }
    }
}