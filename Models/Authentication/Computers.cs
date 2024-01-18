using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Authentication
{
    public class Computers
    {
        [Key]
        public int CId { get; set; }
        public string CName { get; set; }
        public string CIP { get; set; }
        public string CMac { get; set; }
        public string CUser { get; set; }
        public string CRegisterDate { get; set; }
    }
}
