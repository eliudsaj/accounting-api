using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Authentication
{
    public class CustomUsers
    {
        [Key]
        public int UId { get; set; }
        public string UFirstName { get; set; }
        public string ULastName { get; set; }
        public string UEmail { get; set; }
    }
}
