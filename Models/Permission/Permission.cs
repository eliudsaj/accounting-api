using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Permission
{
    public class Permission
    {


        public int PUser { get; set; }

        public int PId { get; set; }
        //Branches
        public bool ViewBranches { get; set; }

        public bool AddBranch { get; set; }

        public bool EditBranch { get; set; }

        public bool DeleteBranch { get; set; }

        ///Currencies
        ///
        public bool ViewCurrency { get; set; }

        public bool AddCurrency { get; set; }

        public bool EditCurrency { get; set; }

        public bool DeleteCurrency { get; set; }

        //Customer
        public bool ReadCustomer { get; set; }

        public bool AddCustomer { get; set; }

        public bool UpdateCustomer { get; set; }

        public bool DeleteCustomer { get; set; }


        //PurchaseCustomer
        public bool ReadPurchaseCustomer { get; set; }



        //VAT

        public bool ReadVAT { get; set; }

        public bool AddVAT { get; set; }

        public bool UpdateVAT { get; set; }

        public bool DeleteVAT { get; set; }

        // INVOICE
        public bool AddInvoice { get; set; }

        public bool ReadInvoices { get; set; }

        //CREDIT NOTE

        public bool AddCredNote { get; set; }
        public bool ReadCreditNote { get; set; }

        //INVENTORY

        public bool ReadInventory { get; set; }

        public bool AddInventory { get; set; }

        public bool ModifyInventory { get; set; }

        //LPO
        public bool ReadLPO { get; set; }

        public bool ManageLPO { get; set; }

        //PURCHASE RECEIPT

        public bool ReadPurchaseReceipts { get; set; }

        public bool ManagePurchaseReceipts { get; set; }

        //PURCHASE REQUEST

        public bool ReadPurchaseRequests { get; set; }

        public bool ReceivePurchaseRequest { get; set; }

        public bool CreatePurchaseRequest { get; set; }

        //STOCK TAKE

        public bool stockTakeRead { get; set; }

        public bool stockTakeCreate { get; set; }

        public bool stockTakeAction { get; set; }

        //PURCHASE RETURN

        public bool ApprovePurchaseReturn { get; set; }

        public bool ReadPurchaseReturn { get; set; }

        public bool CreatePurchaseReturn { get; set; }

        //DEPARTMENT

        public bool ReadDepartments { get; set; }

        public bool ManageDepartments { get; set; }

        //USERS

        public bool ReadUsers { get; set; }

        public bool ManageUsers { get; set; }

        //Permissions
        public bool ReadPermissions { get; set; }

        public bool ManagePermissions { get; set; }

        public bool StockReports { get; set; }

        //Finacial Perioad

        public bool ManageFinancialPeriods { get; set; }

        /// SL Settings
        
        public bool SLSettings { get; set; }


        //PL Settings

        public bool PLSettings { get; set; }

        //Create Quotation

        public bool CreateQuotation { get; set; }

        //Warehouse

        public bool ManageWarehouses { get; set; }

        public bool ManageUnits { get; set; }

        //Categories
        public bool ManageCategories { get; set; }

        //UserGroups
        public bool EditUserGroup { get; set; }

        public bool CreateUserGroup { get; set; }

        public bool ViewUserGroup { get; set; }


        //




    }

    public class PermissionRequest{

        public int user_id { get; set; }

        public  List<string>  settrue{ get; set; }


        public List<string> setfalse { get; set; }





    }

    public class CreateUserPermissionRequest
    {

        public int user_id { get; set; }

        public List<string> settrue { get; set; }
    }



}
