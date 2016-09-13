using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.Email
{
    public class WelcomeModel
    {
        public string Subject { get; set; }
        public string Address { get; set; }
        public string User { get; set; }
    }
}
