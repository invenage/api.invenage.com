using InvenageAPI.Services.Enum;
using InvenageAPI.Services.Global;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace InvenageAPI.Services.Extension
{
    public static class HashExtensions
    {
        public static string ComputeHash(this string input, HashType type = HashType.SHA256)
            => string.Concat(Encoding.UTF8.GetBytes(input).ComputeHash(type).Select(i => i.ToString("x2")));

        public static byte[] ComputeHash(this byte[] input, HashType type = HashType.SHA256)
        {
            HashAlgorithm algorithm = type switch
            {
                HashType.SHA256 => SHA256.Create(),
                HashType.SHA512 => SHA512.Create(),
                _ => throw new NotSupportedException(),
            };

            return algorithm.ComputeHash(input);
        }

        public static string ComputeSaltedHash(this string input, string salt = null, HashType type = HashType.SHA256)
            => $"{(salt.IsNullOrEmpty() ? GlobalVariable.Salt : salt)}{input}".ComputeHash(type);
    }
}
