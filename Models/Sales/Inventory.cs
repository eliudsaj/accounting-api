using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace pyme_finance_api.Models.Sales
{
    public class Inventory
    {
        [Key]
        public int InvtId { get; set; }
        public string InvtType { get; set; }
        public string InvtName { get; set; }
        public int InvtQty { get; set; }
        public decimal InvtBP { get; set; }
        public decimal InvtSP { get; set; }
        public int InvtReorderLevel { get; set; }
        public DateTime InvtDateAdded { get; set; }
        public DateTime InvtDateModified { get; set; }
        public int InvtAddedBy { get; set; }
        public int InvtModifiedBy { get; set; }
        public int InvtCurrency { get; set; }
        public int InvtBranch { get; set; }
        public int InvtCategory { get; set; }
        public string InvtProdCode { get; set; }
        public string InvtRef { get; set; }

        public string InventoryItem { get; set; }

        public string ProdDesc { get; set; }
        public int UOM { get; set; }
        public bool Obsolete { get; set; }
        public bool NonStock { get; set; }
        public string ProdImage { get; set; }
        public string BatchRef { get; set; }
        public bool BOM { get; set; }
        public string StkType { get; set; }
        public int PartsPerUnit { get; set; }
        public string UnitSeparator { get; set; }
        public string SupplierRef { get; set; }
        public int LeadTime { get; set; }
        public string? SLProdGrpCode { get; set; }
        public string? PLProdGrpCode { get; set; }
        public int ProdDiscId { get; set; }
        public decimal ProdDiscPerc { get; set; }
        public decimal UdCostPrice { get; set; }
        public decimal AvgCostPrice { get; set; }
        public decimal LastPrice { get; set; }
        //public double Weight { get; set; }
        public DateTime? LastMovDate { get; set; }
        public DateTime? LastIssueDate { get; set; }
        public string WarehouseRef { get; set; }

        public virtual string AddedFirstname { get; set; }
        public virtual string AddedLastname { get; set; }
        public virtual string ModifiedFirstname { get; set; }
        public virtual string ModifiedLastname { get; set; }
        public virtual string category_name { get; set; }
        public virtual string category_ref { get; set; }
        public virtual string CurrName { get; set; }
        public virtual string CurrCode { get; set; }
        public virtual int InvtVATId { get; set; }
        public virtual float VATPerc { get; set; }
        public virtual string VATRef { get; set; }
        public string warehouse_code { get; set; }
        public string warehouse_descr { get; set; }
       
    }


    public class InventoryRequest
    {
        [Key]
        public int InvtId { get; set; }
        public string InvtType { get; set; }
        public string InvtName { get; set; }
        public int InvtQty { get; set; }
        public decimal InvtBP { get; set; }
        public decimal InvtSP { get; set; }
        public int InvtReorderLevel { get; set; }
        public DateTime InvtDateAdded { get; set; }
        public DateTime InvtDateModified { get; set; }
        public int InvtAddedBy { get; set; }
        public int InvtModifiedBy { get; set; }
        public int InvtCurrency { get; set; }
        public int InvtBranch { get; set; }
        public int InvtCategory { get; set; }
        public string InvtProdCode { get; set; }
        public string InvtRef { get; set; }

        public string ProdDesc { get; set; }
        public string UOM { get; set; }
        public bool Obsolete { get; set; }
        public bool NonStock { get; set; }
        public IFormFile ProdImage { get; set; }
        public string BatchRef { get; set; }
        public bool BOM { get; set; }
        public string StkType { get; set; }
        public int PartsPerUnit { get; set; }
        public string UnitSeparator { get; set; }
        public string SupplierRef { get; set; }
        public int LeadTime { get; set; }
        public string SLProdGrpCode { get; set; }
        public string PLProdGrpCode { get; set; }

        public string InventoryType { get; set; }
        public int ProdDiscId { get; set; }
        public decimal ProdDiscPerc { get; set; }
        public decimal UdCostPrice { get; set; }
        public decimal AvgCostPrice { get; set; }
        public decimal LastPrice { get; set; }
        public double Weight { get; set; }
        public DateTime? LastMovDate { get; set; }
        public DateTime? LastIssueDate { get; set; }
        public string WarehouseRef { get; set; }

        public virtual string AddedFirstname { get; set; }
        public virtual string AddedLastname { get; set; }
        public virtual string ModifiedFirstname { get; set; }
        public virtual string ModifiedLastname { get; set; }
        public virtual string category_name { get; set; }
        public virtual string category_ref { get; set; }
        public virtual string CurrName { get; set; }
        public virtual string CurrCode { get; set; }
        public virtual int InvtVATId { get; set; }
        public virtual float VATPerc { get; set; }
        public virtual string VATRef { get; set; }
        public string warehouse_code { get; set; }
        public string warehouse_descr { get; set; }

    }
}
