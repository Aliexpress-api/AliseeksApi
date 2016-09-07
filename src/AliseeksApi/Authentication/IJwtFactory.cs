using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace AliseeksApi.Authentication
{
    public interface IJwtFactory
    {
        string GenerateToken(Claim[] claims);
    }
}
