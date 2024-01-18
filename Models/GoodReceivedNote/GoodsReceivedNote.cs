using System;
using System.Collections.Generic;

namespace pyme_finance_api.Models.GoodReceivedNote
{
    public class GoodsReceivedNote
    {
        public int id { get; set; }
        public string invoiceId { get; set; }
        public int supplierId { get; set; }  
        public string supplier { get; set; }
        public string SupplierCode { get; set; }
        public string AddressContact { get; set; }
        public string PysicalAddress { get; set; }
        public string VatCode { get; set; }
        public int createdBy { get; set; }
        public string username { get; set; }
        public string Details { get; set; }
        public DateTime createdOn { get; set; }
        public DateTime ReceivedDate { get; set; }
        public List<GoodReceivedNoteProduct> goodReceivedNoteProducts {get; set; } 
        
        public List<GoodReceivedNoteProductResponse> goodReceivedNoteProductResponses { get; set; }
    }

    public class GoodReceivedNoteProduct
    {
        public int ProductId { get; set; }

        public int ProductAmount { get; set; }
    }


    public class GoodReceivedNoteProductResponse
    {
        public string Product { get; set; }

        public int ProductAmount { get; set; }
    }
}
