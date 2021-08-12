using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
    interface IDataIntegrity
    {
        /// <summary>
        /// Kiểm tra tính toàn vẹn của dữ liệu.
        /// </summary>
        /// <param name="signature">Dữ liệu sau khi đã được xử lý băm với <seealso cref="Key"/>.</param>
        /// <param name="data">Dữ liệu cần kiểm tra tính toàn vẹn.</param>
        /// <returns>Đảm bảo toàn vẹn thì trả về <see langword="true"/>, ngược lại trả về <see langword="false"/>.</returns>
        bool IsIntegrity(string data, string signature);

        /// <summary>
        /// Tạo ra chữ ký bằng cách xử lý băm <seealso cref="Key"/> với <paramref name="data"/>
        /// </summary>
        /// <param name="data">Dữ liệu cần truyền đi.</param>
        string GenerateSignature(string data);
    }
}
