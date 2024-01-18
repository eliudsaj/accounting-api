using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Sales.InvoiceConfig
{
    public class InvoiceSettings
    {
        [Key]
        public int InvSettingId { get; set; }
        public string InvPrefix { get; set; }
        public int InvStartNumber { get; set; }
        public string InvNumberingType { get; set; }
        public virtual int LastNumber { get; set; }
        public int InvDeliveryNotes { get; set; }
      
        public int InvTypesCount { get; set; }
    }

    public class MyTerms
    {

        public string terms { get; set; }   
    }
}
