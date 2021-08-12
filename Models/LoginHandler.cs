using Api.Entities;

namespace Api.Models
{
    public static class LoginHandler
    {
        private static BestsvContext db = new BestsvContext();

        /// <summary>
        /// Kiểm tra thông tin đăng nhập
        /// </summary>
        /// <param name="id">Tài khoản</param>
        /// <param name="password">Mật khẩu</param>
        /// <param name="account">Nếu hợp lệ sẽ trả về thông tin Tài Khoản</param>
        /// <returns>true: hợp lệ<br/>false: sai <paramref name="password"/><br/>nul: <paramref name="id"/> không tồn tại</returns>
        public static bool? IsValid(string id, string password, out Account account)
        {
            id = id.ToLower();
            account = db.Accounts.Find(id);
            if (account == null)
                return null;

            return new Cipher(id).IsIntegrity(password, account.Password);
        }
    }
}