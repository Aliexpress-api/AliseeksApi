using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using AliseeksApi.Configuration;
using AliseeksApi.Models.Email;
using System.IO;

namespace AliseeksApi.Services.Email
{
    public class EmailService : IEmailService
    {
        EmailOptions config;
        const string templatePasswordReset = "\\Views\\Templates\\Email\\PasswordReset.html";

        public EmailService(IOptions<EmailOptions> config)
        {
            this.config = config.Value;
        }

        public async Task SendMailTo(string body, string subject, string address)
        {
            await sendMail(body, subject, address);
        }

        public async Task SendPasswordResetTo(PasswordResetModel model)
        {
            try
            {
                using (var stream = new StreamReader(File.OpenRead(Directory.GetCurrentDirectory() + templatePasswordReset)))
                {
                    var template = stream.ReadToEnd();
                    template = template.Replace("{{product_name}}", "Aliseeks");
                    template = template.Replace("{{name}}", model.Name);
                    template = template.Replace("{{action_url}}", model.ActionUrl);
                    template = template.Replace("{{sender_name}}", "Alex");

                    await sendMail(template, model.Subject, model.ToAddress);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        async Task sendMail(string body, string subject, string address)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(config.Name, config.EmailAddress));
            message.To.Add(new MailboxAddress(config.Name, address));
            message.Subject = subject;

            message.Body = new BodyBuilder()
            {
                HtmlBody = body
            }.ToMessageBody();

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
