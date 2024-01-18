using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Authentication
{
    public class License
    {
        [Key]
        public int LsId { get; set; }
        public string LsType { get; set; }
        public string LsCode { get; set; }
        public DateTime LsIssueDate { get; set; }
        public DateTime LsExpireDate { get; set; }

        public string CompanyName { get; set; }
        public string CompanySlogan { get; set; }
        public int CompanyAdmin { get; set; }
        public string CompanyPostal { get; set; }
        public string CompanyContact { get; set; }
        public string CompanyVAT { get; set; }
        public string PhysicalAddress { get; set; }
        public string CompanyLogo { get; set; }
        public int CompanyCurrency { get; set; }
        public string[] PaymentReceiptEmails { get; set; }
    }
}
