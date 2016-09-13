using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Email;

namespace AliseeksApi.Services.Email
{
    public interface IEmailService
    {
        Task SendMailTo(string body, string subject, string address);
        Task SendPasswordResetTo(PasswordResetModel model);
        Task SendWelcomeTo(WelcomeModel model);
    }
}
