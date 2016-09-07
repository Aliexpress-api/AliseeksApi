using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using AliseeksApi.Configuration;

namespace AliseeksApi.Services.Email
{
    public class EmailService : IEmailService
    {
        EmailOptions config;

        public EmailService(IOptions<EmailOptions> config)
        {
            this.config = config.Value;
        }

        public async Task SendMail(string body, string subject)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(config.Name, config.EmailAddress));
            message.To.Add(new MailboxAddress(config.Name, config.EmailAddress));
            message.Subject = subject;

            message.Body = new TextPart()
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.elasticemail.com", 2525, false);

                client.AuthenticationMechanisms.Remove("XOAUTH2");

                await client.AuthenticateAsync(config.ElasticmailUsername, config.ElasticmailSecret);

                await client.SendAsync(message);

                client.Disconnect(true);
            }
        }
    }
}
