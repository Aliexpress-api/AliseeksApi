using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Services.Email
{
    public interface IEmailService
    {
        Task SendMail(string body, string subject);
    }
}
