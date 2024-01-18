using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MimeKit;

namespace pyme_finance_api.Models.Email
{
    public class EmailProp
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string EmailAttachment { get; set; }

        public EmailProp(IEnumerable<string> to, string subject, string content, string attachment)
        {
            To = new List<MailboxAddress>();
            To.AddRange(to.Select(x => new MailboxAddress(x)));
            Subject = subject;
            Content = content;
            EmailAttachment = attachment;
        }
    }
}
