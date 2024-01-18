using Newtonsoft.Json;
using Npgsql;
using pyme_finance_api.Common;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace pyme_finance_api.Service.PermissionService
{


    
    public interface IPermissionService
    {
        public Dictionary<string, string> getAllPermissions();

        public Dictionary<string, bool> getUsersPermissions( int userId);

        public MyResponse updateUserPermission(List<string> settrue,List<string> setfalse, int user_id);
        public MyResponse createUserPermission(List<string> settrue,  int user_id);


    }





    public class PermissionService : IPermissionService
    {

        dbconnection myDbconnection = new dbconnection();

        public string OrganizationId { get; set; }


        public PermissionService(string organizationId)
        {
            OrganizationId = organizationId;
        }


        public Dictionary<string, string> getAllPermissions()
        {

            Dictionary<string, string> Permissions = new Dictionary<string, string>();

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            string query = "SELECT * FROM information_schema.columns WHERE table_name = 'UserPermissions' and data_type = 'boolean'; ";

            cnn.Open();

            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();



            while (sdr0.Read())
            {
             
                string permission_name = (string)sdr0["column_name"];

                Permissions.Add(permission_name,permission_name);
     
            }
            cnn.Close();

            return Permissions;
        }

        public Dictionary<string, bool> getUsersPermissions(int userId)
        {
            Dictionary<string, bool> userpermission = new Dictionary<string, bool>();

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            string query = "SELECT* FROM \"UserPermissions\" WHERE  \"PUser\" = " + userId + " ";

            cnn.Open();

            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            Permission permission = new Permission();


            while (sdr0.Read())
            {
                //


                //Branch
                permission.AddBranch = (bool)sdr0["AddBranch"] ;
                permission.DeleteBranch = (bool)sdr0["DeleteBranch"];
                permission.EditBranch = (bool)sdr0["EditBranch"];
                permission.ViewBranches = (bool)sdr0["ViewBranches"];

                //department
                permission.ManageDepartments = (bool)sdr0["ManageDepartments"];
                permission.ReadDepartments = (bool)sdr0["ReadDepartments"];

                //currencies
                permission.AddCurrency = (bool)sdr0["AddCurrency"];
                permission.EditCurrency = (bool)sdr0["EditCurrency"]; 
                permission.ViewCurrency = (bool)sdr0["ViewCurrency"];
                permission.DeleteCurrency = (bool)sdr0["DeleteCurrency"];

                //CUSTOMER
                permission.DeleteCustomer = (bool)sdr0["DeleteCustomer"];
                permission.ReadCustomer = (bool)sdr0["ReadCustomer"];
                permission.AddCustomer = (bool)sdr0["AddCustomer"];
                permission.UpdateCustomer = (bool)sdr0["UpdateCustomer"];

                //VAT
                permission.DeleteVAT = (bool)sdr0["DeleteVAT"];
                permission.UpdateVAT = (bool)sdr0["UpdateVAT"];
                permission.ReadVAT = (bool)sdr0["ReadVAT"];
                permission.AddVAT = (bool)sdr0["AddVAT"];
                // INvoice
                permission.AddInvoice = (bool)sdr0["AddInvoice"];
                permission.ReadInvoices = (bool)sdr0["ReadInvoices"];
                //cREDIT NOTE
                permission.AddCredNote = (bool)sdr0["AddCredNote"];
                permission.ReadCreditNote = (bool)sdr0["ReadCreditNote"];

                //INVENTORY
                permission.ReadInventory = (bool)sdr0["ReadInventory"];
                permission.AddInventory = (bool)sdr0["AddInventory"];
                permission.ModifyInventory = (bool)sdr0["ModifyInventory"];
                //LPO
                permission.ReadLPO = (bool)sdr0["ReadLPO"];
                permission.ManageLPO = (bool)sdr0["ManageLPO"];
                //purchase receipt
                permission.ReadPurchaseReceipts = (bool)sdr0["ReadPurchaseReceipts"];
                permission.ManagePurchaseReceipts = (bool)sdr0["ManagePurchaseReceipts"];
                permission.ReceivePurchaseRequest = (bool)sdr0["ReceivePurchaseRequest"];
                //purchase request
                permission.ReadPurchaseRequests = (bool)sdr0["ReadPurchaseRequests"];
                permission.ReceivePurchaseRequest = (bool)sdr0["ReceivePurchaseRequest"];
                permission.CreatePurchaseRequest = (bool)sdr0["CreatePurchaseRequest"];
                //stock take
                permission.stockTakeRead = (bool)sdr0["stockTakeRead"];
                permission.stockTakeCreate = (bool)sdr0["stockTakeCreate"];
                permission.stockTakeAction = (bool)sdr0["stockTakeAction"];
                //purchase  return
                permission.ApprovePurchaseReturn = (bool)sdr0["ApprovePurchaseReturn"];
                permission.ReadPurchaseReturn = (bool)sdr0["ReadPurchaseReturn"];
                permission.CreatePurchaseReturn = (bool)sdr0["CreatePurchaseReturn"];
                //users
                permission.ReadUsers = (bool)sdr0["ReadUsers"];
                permission.ManageUsers = (bool)sdr0["ManageUsers"];
                //permissions
                permission.ReadPermissions = (bool)sdr0["ReadPermissions"];
                permission.ManagePermissions = (bool)sdr0["ManagePermissions"];
                //stockreports
                permission.StockReports = (bool)sdr0["StockReports"];
                //Financial Period
                permission.ManageFinancialPeriods = (bool)sdr0["ManageFinancialPeriods"];

                //SLsettings
                permission.SLSettings = (bool)sdr0["SLSettings"];
                //PLsettings
                permission.PLSettings = (bool)sdr0["PLSettings"];
                //CreateQuotation
                permission.CreateQuotation = (bool)sdr0["CreateQuotation"];

                //PurchaseCustomers
                permission.ReadPurchaseCustomer = (bool)sdr0["ReadPurchaseCustomer"];
                //Warehouse
                permission.ManageWarehouses = (bool)sdr0["ManageWarehouses"];
                //Units
                permission.ManageUnits = (bool)sdr0["ManageUnits"];
                //Categories
                permission.ManageCategories = (bool)sdr0["ManageCategories"];
                //userGroups
                permission.CreateUserGroup = (bool)sdr0["CreateUserGroup"];
                permission.EditUserGroup = (bool)sdr0["EditUserGroup"];
                permission.ViewUserGroup = (bool)sdr0["ViewUserGroup"];

            }

            cnn.Close();
            var allpermissions = this.getAllPermissions();

            var properties = permission.GetType().GetProperties();
            foreach (var prop in properties)
            {
                if (allpermissions.ContainsKey(prop.Name))
                {
                    string Prop = prop.Name;
                    userpermission.Add(prop.Name, (bool)permission.GetType().GetRuntimeProperty(Prop).GetValue(permission));
                }
                else
                {
                    //EditCustomer
                    Console.WriteLine($"prop {prop.Name} is not the available");
                }
            }

            Console.WriteLine(userpermission);

            return userpermission;

        }

        public MyResponse updateUserPermission(List<string> settrue, List<string> setfalse, int user_id)
        {
            //check if settrue is null
            int set_true_count = settrue.Count;
            int set_false_count =  setfalse.Count;
            StringBuilder set_true_update_query = new StringBuilder(); //string will be appended later
            StringBuilder set_false_update_query = new StringBuilder(); //string will be appended later
            bool myReq1 = true;
            bool myReq2 = true;

            MyResponse response = new MyResponse();


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            cnn.Open();
      
               
                

                    if(set_true_count > 0)
                    {
                        set_true_update_query.Append("UPDATE  \"UserPermissions\" SET ");

                        for (int i = 0; i <=( set_true_count-1); i++)
                        {


                              if ((set_true_count - 1) == i)
                            {
                                set_true_update_query.Append(String.Format("  \"{0}\"  = {1} ", settrue[i], true));
              

                            }
                            else {
                              set_true_update_query.Append(String.Format(" \"{0}\"  = {1} ,", settrue[i], true));

                            }
                        }

                            set_true_update_query.Append(" WHERE \"PUser\" = '" + user_id + "' ; ");


                Console.WriteLine("QUERY 2 ======================================================================="+ set_true_update_query.ToString());

                myDbconnection.UpdateDelInsert(set_true_update_query.ToString(), OrganizationId);


            }
                

            if (set_false_count > 0)
                    {
                         set_false_update_query.Append("UPDATE  \"UserPermissions\" SET ");

                        for (int i = 0; i <= (set_false_count-1); i++){
                                  string item = setfalse[i];
                                  
                          
                                    if ((set_false_count - 1) == i)
                                    {
                                        set_false_update_query.Append(String.Format(" \"{0}\"  = {1} ", setfalse[i], false));
                     

                                     }
                                    else
                                    {
                        set_false_update_query.Append(String.Format(" \"{0}\"  = {1} ,", setfalse[i], false));

                                     }
                        }


                set_false_update_query.Append(" WHERE \"PUser\" = '" + user_id + "' ; ");



                Console.WriteLine("QUERY 1 ========================================================================"+ set_false_update_query.ToString());
                myReq1 = myDbconnection.UpdateDelInsert(set_false_update_query.ToString(), OrganizationId);

            }
           

            //string query = set_false_update_query.ToString();
          //  string query2 = set_true_update_query.ToString();

            //Console.WriteLine("QUERY 1 ========================================================================"+query);

            //Console.WriteLine("QUERY 2 ======================================================================="+query2);
            try
            {

            
            // myReq1 = myDbconnection.UpdateDelInsert(query, OrganizationId);
         //   bool myReq2 = myDbconnection.UpdateDelInsert(query2, OrganizationId);



            if (myReq1 == false || myReq2 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";

                return response;
            }
            //if (myReq2 == false)
            //{
            //    response.Httpcode = 400;
            //    response.Message = "An occured while trying to save details.";

            //    return response;
            //}


            response.Httpcode = 200;
            response.Message = "success";

            return response;
            }
            catch (Exception e)
            {
               Console.WriteLine($"Error  is {e.Message}");

                response.Httpcode = 400;
                response.Message = e.Message;

                return response;
            }
        }

        public MyResponse createUserPermission(List<string> settrue, int user_id)
        {
            StringBuilder query = new StringBuilder(); //string will be appended later
            MyResponse response = new MyResponse();
            int set_true_count = settrue.Count;

            ///Credit entries(vat,sales)
            ///

            ///chcek if user has permissions
            string check_if_user_exists = " SELECT * FROM \"UserPermissions\" WHERE \"PUser\" = '"+user_id+"' ";
           int count = myDbconnection.CheckRowExists(check_if_user_exists, OrganizationId);
            if(count >= 1)
            {
                response.Httpcode = 400;
                response.Message = "User already has permissions saved";
                return response;
            }


            query.Append("INSERT INTO \"UserPermissions\"  (\"PUser\", ");

            for (int i = 0; i <= (set_true_count - 1); i++)
            {
                string item = settrue[i];


                if ((set_true_count - 1) == i)
                {
                    query.Append(String.Format(" \"{0}\")", item));


                }
                else
                {
                    query.Append(String.Format(" \"{0}\" ,", item));

                }
            }
            query.Append("VALUES ('" + user_id + " ' ,");
            for (int i = 0; i <= (set_true_count - 1); i++)
            {
                string item = settrue[i];


                if ((set_true_count - 1) == i)
                {
                    query.Append(String.Format(" {0} );", true));


                }
                else
                {
                    query.Append(String.Format(" {0} ,", true));

                }
            }

            string finalquery = query.ToString();
            bool myReq1 = myDbconnection.UpdateDelInsert(finalquery, OrganizationId);

            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";

                return response;
            }
            response.Httpcode = 200;
            response.Message = "Permissions were added successfully";

            return response;
        }
    }

}
