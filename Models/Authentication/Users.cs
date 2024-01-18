using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Authentication
{
    public class Users
    {
        [Key]
        public int UId { get; set; }
        public string UFirstName { get; set; }
        public string ULastName { get; set; }
        public string UEmail { get; set; }

        public string Username { get; set; }
        public string UPassword { get; set; }
        public string UType { get; set; }
        public string UCompany { get; set; }
        public string UContact { get; set; }
        public string UStatus { get; set; }
        public string UProfile { get; set; }
        public int UDepartment { get; set; }
        public int UBranch { get; set; }
        public bool UFirst { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int UIdnumber { get; set; }
        public string UVAT { get; set; }

        public virtual string Department_name { get; set; }
        public virtual string Department_ref { get; set; }
        public virtual string Branch_name { get; set; }

    }
}
