using pyme_finance_api.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.StockInv
{
    public class QuotationRequest
    {
        public string pr_additional { get; set; }
        public string pr_customer { get; set; }

        public int pr_createdby { get; set; }
        public DateTime pr_date { get; set; }
        public List<InventoryItem> pr_details { get; set; }
    }





    public class DeliveryNoteRequest
    {
        public string pr_additional { get; set; }
        public int pr_customer { get; set; }

        public string lpo { get; set; }
        public string status { get; set; }  
        public int pr_createdby { get; set; }
        public DateTime pr_date { get; set; }
        public List<InventoryItem> pr_details { get; set; }
    }

    public class ApproveQuotation
    {
        public int userId { get; set; }

        public int quotationId { get; set; }
    }


    public class InventoryItem{


        public string pd_item_name { get; set; }

        public string image_path { get; set; }
        public double pd_unitprice { get; set; }

        public int pd_qty { get; set; }


        public int pd_item { get; set; }


        public double pd_totals { get; set; }

        public string pd_vat_perc { get; set; }

        public double pd_vat_amt { get; set; }


    }
    public class Quotation
    {
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime QuotationDate { get; set; }

        public string Status { get; set; }

        public string Details { get; set; }

        public int CreatedBy { get; set; }

        public int ActionBy { get; set; }

        public License license { get; set; }

        public List<QuotationDetails> QuotationDetails { get; set; }
    }

    public class DeliveryNote
    {

        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public int Customer { get; set; }

        public string  Lpo { get; set; }

        public string Details { get; set; }

        public int CreatedBy { get; set; }

        public int ActionBy { get; set; }

        public License license { get; set; }

        public List<QuotationDetails> QuotationDetails { get; set; }



    }





    public class QuotationDetails
    {
        public int Id { get; set; }

        public int Quotation_ref { get; set; }

        public string VatPerc { get; set; }

        public decimal VatAmt { get; set; }

        public string StkDesc { get; set; }

        public int ProdQty { get; set; }

        public decimal Total { get; set; }

        public decimal UnitPrice { get; set; }

    }









}
