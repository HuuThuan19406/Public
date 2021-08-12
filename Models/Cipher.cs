using System.Security.Cryptography;
using System.Text;

namespace Api.Models
{
    public class Cipher : IDataIntegrity
    {
        private readonly string key;
        public byte Level { get; set; }

        public Cipher(string key, byte level = 100)
        {
            this.key = key;
            Level = level;
        }

        public string GenerateSignature(string data)
        {
            string code = data + key + data;
            SHA512 sHA512 = SHA512.Create();

            for (int i = 0; i < Level; i++)
            {
                code = HashBytesToString(sHA512.ComputeHash(Encoding.ASCII.GetBytes(code + key)));
            }

            return code;
        }

        private string HashBytesToString(byte[] hashBytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in hashBytes)
            {
                sb.Append(item.ToString("X2"));
            }
            return sb.ToString();
        }

        public bool IsIntegrity(string data, string signature)
        {
            return GenerateSignature(data).Equals(signature);
        }
    }
}