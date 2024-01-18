using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace pyme_finance_api.Models.Email
{
    public class EmailFunctions
    {
        private IConfiguration _configuration;

        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public EmailFunctions(IConfiguration config)
        {
            _configuration = config;

           
        }

        public void SendEmail(EmailProp message)
        {
            var email_message = createEmailMessage(message);
            Send(email_message);
        }

        private MimeMessage createEmailMessage(EmailProp message)
        {
            EmailConfiguration eml_config = new EmailConfiguration(_configuration);

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(MailboxAddress.Parse(eml_config.from));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;

            var bodybuilder = new BodyBuilder
            {
                HtmlBody = message.Content
            };

            //check if has attachment
            if (!string.IsNullOrEmpty(message.EmailAttachment))
            {
                byte[] file_bytes = System.Convert.FromBase64String(message.EmailAttachment);
                bodybuilder.Attachments.Add("filenameAttachment", file_bytes, ContentType.Parse("image/jpeg"));
            }


           // emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) {Text = message.Content};
           emailMessage.Body = bodybuilder.ToMessageBody();
            return emailMessage;

        }

        private void Send(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient() )
            {
                try
                {
                    EmailConfiguration eml_config = new EmailConfiguration(_configuration);

                    using var smtp = new SmtpClient();
                    smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    smtp.Connect(eml_config.smtp_server, eml_config.port, SecureSocketOptions.Auto);
                    smtp.Authenticate(eml_config.username, eml_config.password);
                    smtp.Send(mailMessage);
                    smtp.Disconnect(true);


                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }
    }
}
