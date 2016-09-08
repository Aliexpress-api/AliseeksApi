using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace AliseeksApi.Utility.Security
{
    public class SecurityHasher : ISecurityHasher
    {
        public HashCode Hash(string value)
        {
            string password = value;

            //Generate a 128-bit salt
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hash = new HashCode();

            hash.Salt = Convert.ToBase64String(salt);

            hash.Hash = hashValue(value, salt);

            return hash;
        }

        public HashCode HashWithSalt(string value, string salt)
        {
            var hash = new HashCode();
            var chars = salt.ToCharArray();
            
            hash.Salt = salt;

            hash.Hash = hashValue(value, Convert.FromBase64CharArray(chars, 0, chars.Length));

            return hash;
        }

        string hashValue(string value, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: value,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 100,
                numBytesRequested: 256 / 8
                ));
        }
    }

    public class HashCode
    {
        public string Salt { get; set; }
        public string Hash { get; set; }
    }
}
