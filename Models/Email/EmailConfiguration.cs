using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace pyme_finance_api.Models.Email
{
    public class EmailConfiguration
    {
        public string from { get; set; }
        public string smtp_server { get; set; }
        public int port { get; set; }
        public string username { get; set; }
        public string password { get; set; }

        private IConfiguration _configuration;
        public EmailConfiguration(IConfiguration config)
        {
            _configuration = config;

            //assign data
            from = _configuration["MailSettings:SendEmail"];
            smtp_server = _configuration["MailSettings:Host"];
            port = int.Parse(_configuration["MailSettings:Port"]);
            username = _configuration["MailSettings:SendEmail"];
            password = _configuration["MailSettings:Password"];
        }


    }
}
