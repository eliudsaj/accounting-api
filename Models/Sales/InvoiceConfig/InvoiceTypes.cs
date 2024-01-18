using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Sales.InvoiceConfig
{
    public class InvoiceTypes
    {
        [Key]
        public int INVypeID { get; set; }
        public string INVType { get; set; }
        public string INVComment { get; set; }
    }

    public class CreditNoteType
    {
        [Key]
        public int CRNId { get; set; }
        public string CRNType { get; set; }
        public string CRNComment { get; set; }
    }
    public class GoodReturnNoteType
    {
        [Key]
        public int GRNId { get; set; }
        public string GRNType { get; set; }
        public string GRNComment { get; set; }
    }
    public class PurchaseHeaderSettings
    {
        public int Id { get; set; }
        public string DocumentName { get; set; }
        public string Category { get; set; }
        public bool Status { get; set; } = false;
    }
}
