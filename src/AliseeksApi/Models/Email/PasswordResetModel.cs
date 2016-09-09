using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.Email
{
    public class PasswordResetModel
    {
        public string ToAddress { get; set; }
        public string Subject { get; set; }
        public string Name { get; set; }
        public string ActionUrl { get; set; }
        public string SenderName { get; set; }
    }
}
