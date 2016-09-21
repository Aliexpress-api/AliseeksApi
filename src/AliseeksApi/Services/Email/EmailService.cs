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
using AliseeksApi.Models.Logging;
using AliseeksApi.Services.Logging;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using SharpRaven.Core;
using SharpRaven.Core.Data;
using AliseeksApi.Utility.Extensions;   

namespace AliseeksApi.Services.Email
{
    public class EmailService : IEmailService
    {
        EmailOptions config;
        ILoggingService logging;

        //const string templatePasswordReset = @"\Views\Templates\Email\PasswordReset.html";
        string templatePasswordReset = Directory.GetCurrentDirectory() + Path.Combine(new string[] { Directory.GetCurrentDirectory(), "Views", "Templates", "Email", "PasswordReset.html" });
        string templateWelcome = Path.Combine(new string[] {Directory.GetCurrentDirectory(), "Views", "Templates", "Email", "Welcome.html" });
        //string templateWelcome = @"\Views\Templates\Email\Welcome.html";

        private readonly IRavenClient raven;

        public EmailService(IOptions<EmailOptions> config, ILoggingService logging, IRavenClient raven)
        {
            this.config = config.Value;
            this.logging = logging;
            this.raven = raven;
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
                var sentry = new SentryEvent(e);
                await raven.CaptureNetCoreEventAsync(sentry);
            }
        }

        public async Task SendWelcomeTo(WelcomeModel model)
        {
            try
            {
                using (var stream = new StreamReader(File.OpenRead(Directory.GetCurrentDirectory() + templateWelcome)))
                {
                    var template = stream.ReadToEnd();
                    template = template.Replace("{{product_name}}", "Aliseeks");
                    template = template.Replace("{{name}}", model.User);
                    template = template.Replace("{{sender_name}}", "Alex");

                    await sendMail(template, model.Subject, model.Address);
                }
            }
            catch (Exception e)
            {
                var sentry = new SentryEvent(e);
                await raven.CaptureNetCoreEventAsync(sentry);
            }
        }

        async Task sendMail(string body, string subject, string address)
        {
            if (!config.SendEmail) { return; }

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
                try
                {
                    client.ServerCertificateValidationCallback = BypassValidateServerCertificate;
                    await client.ConnectAsync("smtp.elasticemail.com", 2525, false);

                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    await client.AuthenticateAsync(config.ElasticmailUsername, config.ElasticmailSecret);

                    await client.SendAsync(message);

                    client.Disconnect(true);
                }
                catch(Exception e)
                {
                    //Log exception through Postgres
                    var model = new ExceptionLogModel()
                    {
                        Message = e.Message + $" {config.ElasticmailUsername} {config.ElasticmailSecret}",
                        Criticality = 10,
                        StackTrace = e.StackTrace
                    };

                    await logging.LogException(model);

                    //Log exception through Sentry
                    var sentry = new SentryEvent(e);
                    sentry.Tags.Add("Elasticmail Config", $"Username: {config.ElasticmailUsername} Secret: {config.ElasticmailSecret}");
                    await raven.CaptureNetCoreEventAsync(sentry);
                }
            }
        }

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool BypassValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
