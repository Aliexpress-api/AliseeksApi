using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Utility.Security
{
    public interface ISecurityHasher
    {
        HashCode Hash(string value);
        HashCode HashWithSalt(string value, string salt);
    }
}
