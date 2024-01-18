using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Sales
{
    public class InvoiceListDetailsData
    {
        public virtual string Description { get; set; }
        public virtual string additionalDetails { get; set; }
        public virtual decimal ItemTotals { get; set; }
        public virtual int Quantity { get; set; }
        public virtual decimal VAT { get; set; }
        public virtual decimal Price { get; set; }
        public virtual decimal Total { get; set; }
        public virtual decimal DiscountAmt { get; set; }
        public virtual decimal Discount { get; set; }

        public string nlaccount { get; set; }
        public string VATPL { get; set; }

        public int SLJrnlNo { get; set; }
        public int JrnlSLNo { get; set; }
        public float InvAmt { get; set; }
        public decimal VatCode { get; set; }
        public decimal VatAmt { get; set; }
        public int ProdGroupCode { get; set; }
        public string NLAccCode { get; set; }
        public string StkDesc { get; set; }
        public int UserID { get; set; }
        [Key]
        public string ItemSerial { get; set; }
        public string ItemCode { get; set; }
        public int ItemId { get; set; }

        //saving from Pl create Invoice
        public int ItemQty { get; set; }
        public decimal ItemUnitPrice { get; set; }

        public string AccountName { get; set; }


        public string account { get; set; }


    }

    public class Inventorylist
    {
        public string chk { get; set; }
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string StkDesc { get; set; }
        public string ItemUnitPrice { get; set; }
        public string ItemQty { get; set; }
        public int VatCode { get; set; }
        public decimal VatAmt { get; set; }
        public string DiscountId { get; set; }
        public decimal DiscountAmt { get; set; }
        public string RowTotal { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public int Discount { get; set; }

        public string SlProductGroup { get; set; }
    }
}
