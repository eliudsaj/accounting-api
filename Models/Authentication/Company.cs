using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Authentication
{
    public class Company
    {
        [Key]
        public int CpId { get; set; }
        public string CpName { get; set; }
        public string CpAddress { get; set; }
        public string CpAdminEmail { get; set; }
        public DateTime CpRegisterDate { get; set; }
        public string CpRef { get; set; }
        public string CpConnString { get; set; }
        public string CpStatus { get; set; }
        public string CpLogo { get; set; }
        public string CpDocuments { get; set; }
        public string CpAdminFirstname { get; set; }
        public string CpAdminLastname { get; set; }
        public string CpAdminContact { get; set; }
      
        public string CpLicenseType { get; set; }
        public DateTime CpExpireDate { get; set; }
        public string CpLicense { get; set; }
        public string KRAPin { get; set; }

        public string CpAdminIP { get; set; }
        public string CpAdminMac { get; set; }
    }
}
