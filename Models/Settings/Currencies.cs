using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Settings
{
    public class Currencies
    {
        [Key]
        public int CrId { get; set; }
        public string CrName { get; set; }
        // [unique]
        public string CrCode { get; set; }
        public string CrCountry { get; set; }
        public string CrStatus { get; set; }
        public DateTime CrCreatedDate { get; set; }
        public DateTime CrModifiedDate { get; set; }
    }
}
