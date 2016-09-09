using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.User
{
    public class UserResetValidModel
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
