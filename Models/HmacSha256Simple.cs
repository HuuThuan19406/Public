using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Api.Models
{
    public class HmacSha256Simple : IDataIntegrity
    {
        private readonly byte[] key;
        private readonly HMACSHA256 hmac;

        public HmacSha256Simple(string key)
        {
            this.key = Encoding.Unicode.GetBytes(key);
            hmac = new HMACSHA256(this.key);
        }

        public string GenerateSignature(string data)
        {
            hmac.ComputeHash(Encoding.Unicode.GetBytes(data));
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hmac.Hash.Length; i++)
            {
                sb.Append(hmac.Hash[i].ToString("x2"));
            }

            return sb.ToString();
        }

        public bool IsIntegrity(string data, string signature)
        {
            return GenerateSignature(data).Equals(signature);
        }
    }
}
