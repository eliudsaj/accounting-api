using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Npgsql;
using Org.BouncyCastle.Ocsp;
using pyme_finance_api.Common;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.NL.NlAccountGroup;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Models.StockInv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pyme_finance_api.Service.InventoryService
{

    public interface IInventoryService
    {
        List<Inventory> GetInventoryList();
        MyResponse AddInventory(InventoryRequest inventoryData, int userId, int staff_branch, int CompanyCurrency);
    }


    public class InventoryService : IInventoryService
    {
        dbconnection myDbconnection = new dbconnection();
        private IWebHostEnvironment _hostingEnvironment;
        public string OrganizationId { get; set; }
        public InventoryService(string organizationId, IWebHostEnvironment environment)
        {
            OrganizationId = organizationId;
            _hostingEnvironment = environment;
        }
        public MyResponse saveQuotation(QuotationRequest recvData)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            MyResponse response = new MyResponse();
            cnn.Open();
            int id = 0;
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    string status = "PENDING";
                    string insertquery1 = "INSERT INTO \"Quotations\" (\"CreatedOn\",\"QuotationDate\",\"Status\",\"Details\",\"CreatedBy\") " +
                     "VALUES('" + DateTime.Now + "','" + recvData.pr_date + "','" + status + "', '" + recvData.pr_additional + "', '" + recvData.pr_createdby + "') RETURNING \"Id\" ;";

                    var cmd = new NpgsqlCommand(insertquery1, cnn, trans);
                    id = int.Parse(cmd.ExecuteScalar().ToString());
                    StringBuilder stringBuilder = new StringBuilder();

                    foreach (var item in recvData.pr_details)
                    {
                        stringBuilder.Append($"INSERT INTO \"QuotationDetails\" (\"Quotation_ref\",\"VatPerc\",\"VatAmt\",\"StkDesc\",\"ProdQty\",\"Total\",\"UnitPrice\") " +
                            $"VALUES({id},{item.pd_vat_perc},{item.pd_vat_amt},'" + item.pd_item_name + "','" + item.pd_qty + "','" + item.pd_totals + "','" + item.pd_unitprice + "');");
                    }
                    cmd.CommandText = stringBuilder.ToString();
                    //cmd.CommandText = creditentry2;
                    //cmd.CommandText = debitentry1;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    response.Httpcode = 200;
                    response.Message = "success";
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    cnn.Dispose();
                    cnn.CloseAsync();
                    response.Httpcode = 400;
                    Console.WriteLine(e.Message);
                    response.Message = "An error occoured , please try again later";
                    throw e;
                }
            }
            cnn.CloseAsync();
            return response;
        }
        public MyResponse DeliveryNotenonstock(DeliveryNoteRequest recvData)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            MyResponse response = new MyResponse();
            cnn.Open();
            int id = 0;
            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    string status = "PENDING";
                    string insertquery1 = "INSERT INTO \"DeliveryNotes\" (\"CreatedOn\",\"Lpo\",\"Status\",\"Details\",\"CreatedBy\",\"CustomerId\") " +
                     "VALUES('" + DateTime.Now + "','" + recvData.lpo + "','" + status + "', '" + recvData.pr_additional + "', '" + recvData.pr_createdby + "','" + recvData.pr_customer + "') RETURNING \"Id\" ;";

                    var cmd = new NpgsqlCommand(insertquery1, cnn, trans);
                    id = int.Parse(cmd.ExecuteScalar().ToString());
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var item in recvData.pr_details)
                    {
                        stringBuilder.Append($"INSERT INTO \"DeliveryNoteDetails\" (\"DeliveryNote_ref\",\"StkDesc\",\"ProdQty\",\"Total\",\"UnitPrice\") " +
                            $"VALUES({id},'" + item.pd_item_name + "','" + item.pd_qty + "','" + item.pd_totals + "','" + item.pd_unitprice + "');");
                    }
                    cmd.CommandText = stringBuilder.ToString();
                    //cmd.CommandText = creditentry2;
                    //cmd.CommandText = debitentry1;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    response.Httpcode = 200;
                    response.Message = "success";
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    cnn.Dispose();
                    cnn.CloseAsync();
                    response.Httpcode = 400;
                    Console.WriteLine(e.Message);
                    response.Message = "An error occurred , please try again later";
                }
            }
            cnn.CloseAsync();
            return response;
        }
        public MyResponse AddInventory(InventoryRequest inventoryData, int userId, int staff_branch, int CompanyCurrency)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            // NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            MyResponse response = new MyResponse();
            //get guid
            string int_Ref = System.Guid.NewGuid().ToString("D");
            //check if name exists
            cnn.OpenAsync();
            NpgsqlDataReader sdr78 = new NpgsqlCommand("Select * From \"Inventory\" WHERE \"InvtName\" = '" + inventoryData.InvtName + "' ", cnn).ExecuteReaderAsync().Result;
            if (sdr78.HasRows == true)
            {
                response.Httpcode = 404;
                response.Message = "The inventory name " + inventoryData.InvtName + " is already registered.";
                cnn.Close();
                return response;
            }
            cnn.Close();
            //check prod code
            if (!String.IsNullOrEmpty(inventoryData.InvtProdCode))
            {
                cnn.Open();
                NpgsqlDataReader sdr79 = new NpgsqlCommand("Select * From \"Inventory\" WHERE \"InvtProdCode\" = '" + inventoryData.InvtProdCode + "' ", cnn).ExecuteReaderAsync().Result;
                if (sdr79.HasRows == true)
                {
                    response.Httpcode = 404;
                    response.Message = "The inventory code for the inventory " + inventoryData.InvtName + " is already registered.";
                    cnn.Close();
                    return response;
                }
                cnn.Close();
            }
            string img_path = "";
            if (inventoryData.ProdImage != null)
            {
                img_path = SaveImage(inventoryData.ProdImage);
            }
            // savefile
            // create inventory
            cnn.Open();
            string insertQ = "INSERT INTO \"Inventory\" ( \"InvtType\", \"InvtName\", \"InvtQty\", \"InvtReorderLevel\", \"InvtDateAdded\", \"InvtDateModified\", \"InvtAddedBy\", \"InvtModifiedBy\", \"InvtCurrency\", \"InvtVATId\", \"InvtBranch\", \"InvtCategory\", \"InvtProdCode\", \"InvtRef\",    " +
                "\"InvtBP\", \"InvtSP\",  \"ProdDesc\",  \"UOM\",  \"Obsolete\",  \"NonStock\",  \"ProdImage\",   \"BatchRef\", \"BOM\", \"StkType\", \"PartsPerUnit\", \"UnitSeparator\", \"SupplierRef\", \"LeadTime\", \"SLProdGrpCode\", \"PLProdGrpCode\", \"ProdDiscId\", \"UdCostPrice\", \"AvgCostPrice\",  " +
                "\"LastPrice\", \"Weight\", \"LastMovDate\", \"LastIssueDate\", \"WarehouseRef\" ,\"InventoryItem\")  " +
                "VALUES('" + inventoryData.InvtType + "', '" + inventoryData.InvtName + "', " + inventoryData.InvtQty + ", " + inventoryData.InvtReorderLevel + " ,'" + DateTime.Today + "','" + DateTime.Today + "'," + userId + "," + userId + ", " + 1 + "," + inventoryData.InvtVATId + ", " + staff_branch + ",   " +
                " " + inventoryData.InvtCategory + ", '" + inventoryData.InvtProdCode + "','" + int_Ref + "', " + inventoryData.InvtBP + ", " + inventoryData.InvtSP + ", '" + inventoryData.ProdDesc + "', '" + inventoryData.UOM + "', '" + inventoryData.Obsolete + "', '" + inventoryData.NonStock + "',     " +
                " '" + img_path + "', '" + inventoryData.BatchRef + "', '" + inventoryData.BOM + "', '" + inventoryData.StkType + "', " + inventoryData.PartsPerUnit + ", '" + inventoryData.UnitSeparator + "', '" + inventoryData.SupplierRef + "', " + inventoryData.LeadTime + ",    " +
                " '" + inventoryData.SLProdGrpCode + "','" + inventoryData.PLProdGrpCode + "', " + inventoryData.ProdDiscId + ", " + inventoryData.InvtSP + "," + inventoryData.InvtSP + "," + inventoryData.InvtSP + "," + inventoryData.Weight + ",'" + DateTime.Now + "','" + DateTime.Now + "',     " +
                " '" + inventoryData.WarehouseRef + "','" + inventoryData.InventoryType + "'  ) RETURNING \"InvtRef\"; ";
            var cmd = new NpgsqlCommand(insertQ, cnn);
            response.Message = cmd.ExecuteScalar().ToString();
            response.Httpcode = 200;
            // bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, OrganizationId);
            cnn.Close();
            return response;
        }
        public MyResponse RejectQuotation(int quotationid, int actionby)
        {
            MyResponse response = new MyResponse();
            string status = "REJECTED";
            string query = "UPDATE \"Quotations\" SET \"Status\" = '" + status + "' , \"ActionBy\" = " + actionby + "   WHERE \"Id\" = " + quotationid + " ;";
            bool myReq2 = myDbconnection.UpdateDelInsert(query, OrganizationId);
            if (myReq2 == false)
            {
                response.Message = "An error occoured please ,contact administrator";
                response.Httpcode = 400;
            }
            response.Message = "Quotation has been rejected successfully";
            response.Httpcode = 200;
            return response;
        }
        public MyResponse ApproveQuotation(int quotationid, int actionby)
        {
            MyResponse response = new MyResponse();
            string status = "APPROVED";
            string query = "UPDATE \"Quotations\" SET \"Status\" = '" + status + "' , \"ActionBy\" = " + actionby + "   WHERE \"Id\" = " + quotationid + " ;";
            bool myReq2 = myDbconnection.UpdateDelInsert(query, OrganizationId);
            if (myReq2 == false)
            {
                response.Message = " An error occoured please ,contact administrator";
                response.Httpcode = 400;
            }
            response.Message = "Quotation has been approved successfully";
            response.Httpcode = 200;
            return response;
        }
        public List<Quotation> getAllQuotation()
        {
            List<Quotation> quotations = new List<Quotation>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"Quotations\" ", cnn).ExecuteReader();
            NlaccountGroup nlaccountGroup = new NlaccountGroup();
            while (sdr0.Read())
            {
                Quotation quotation = new Quotation();
                quotation.Id = sdr0["Id"] != DBNull.Value ? (int)sdr0["Id"] : 0;
                quotation.CreatedOn = sdr0["CreatedOn"] != DBNull.Value ? (DateTime)sdr0["CreatedOn"] : DateTime.Now;
                quotation.CreatedBy = sdr0["CreatedBy"] != DBNull.Value ? (int)sdr0["CreatedBy"] : 0;
                quotation.QuotationDate = sdr0["QuotationDate"] != DBNull.Value ? (DateTime)sdr0["QuotationDate"] : DateTime.Now;
                quotation.Status = sdr0["Status"] != DBNull.Value ? (string)sdr0["Status"] : "";
                quotation.Details = sdr0["Details"] != DBNull.Value ? (string)sdr0["Details"] : "";
                quotations.Add(quotation);
            }
            cnn.Close();
            return quotations;
        }
        public List<DeliveryNote> getAllDeliveryNotes()
        {
            List<DeliveryNote> quotations = new List<DeliveryNote>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            string query = "SELECT \"DeliveryNotes\".* , \"SLCustomer\".*   FROM \"DeliveryNotes\" LEFT JOIN \"SLCustomer\" ON \"DeliveryNotes\".\"CustomerId\" =  \"SLCustomer\".\"SLCustomerSerial\" ";
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            NlaccountGroup nlaccountGroup = new NlaccountGroup();
            while (sdr0.Read())
            {
                DeliveryNote quotation = new DeliveryNote();
                quotation.Id = sdr0["Id"] != DBNull.Value ? (int)sdr0["Id"] : 0;
                quotation.Lpo = sdr0["Lpo"] != DBNull.Value ? (string)sdr0["Lpo"] : "";
                quotation.CreatedBy = sdr0["CreatedBy"] != DBNull.Value ? (int)sdr0["CreatedBy"] : 0;
                quotation.CreatedOn = sdr0["CreatedOn"] != DBNull.Value ? (DateTime)sdr0["CreatedOn"] : DateTime.Now;
                quotation.Details = sdr0["Details"] != DBNull.Value ? (string)sdr0["Details"] : "";
                quotations.Add(quotation);
            }
            cnn.Close();
            return quotations;
        }

        public DeliveryNote getDeliveryNoteDetail(int id)
        {
            DeliveryNote quotation = new DeliveryNote();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            string query = "SELECT \"DeliveryNotes\".* , \"SLCustomer\".*   FROM \"DeliveryNotes\" LEFT JOIN \"SLCustomer\" ON \"DeliveryNotes\".\"CustomerId\" =  \"SLCustomer\".\"SLCustomerSerial\" WHERE \"DeliveryNotes\".\"Id\" = '" + id + "'";
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            NlaccountGroup nlaccountGroup = new NlaccountGroup();
            while (sdr0.Read())
            {
                quotation.Id = sdr0["Id"] != DBNull.Value ? (int)sdr0["Id"] : 0;
                quotation.Lpo = sdr0["Lpo"] != DBNull.Value ? (string)sdr0["Lpo"] : "";
                quotation.CreatedBy = sdr0["CreatedBy"] != DBNull.Value ? (int)sdr0["CreatedBy"] : 0;
                quotation.CreatedOn = sdr0["CreatedOn"] != DBNull.Value ? (DateTime)sdr0["CreatedOn"] : DateTime.Now;
                quotation.ActionBy = sdr0["CustomerId"] != DBNull.Value ? (int)sdr0["CustomerId"] : 0;
                quotation.Details = sdr0["Details"] != DBNull.Value ? (string)sdr0["Details"] : "";
                quotation.Lpo = sdr0["Lpo"] != DBNull.Value ? (string)sdr0["Lpo"] : "";
                quotation.Details = sdr0["Details"] != DBNull.Value ? (string)sdr0["Details"] : "";
            }
            cnn.Close();
            cnn.Open();
            string query1 = "SELECT * FROM \"DeliveryNoteDetails\" WHERE  \"DeliveryNote_ref\" = '" + id + "' ";
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query1, cnn).ExecuteReader();
            List<QuotationDetails> quotationDetails1 = new List<QuotationDetails>();
            while (sdr1.Read())
            {
                QuotationDetails quotationDetails = new QuotationDetails();
                quotationDetails.StkDesc = sdr1["StkDesc"] != DBNull.Value ? (string)sdr1["StkDesc"] : "";
                quotationDetails.ProdQty = sdr1["ProdQty"] != DBNull.Value ? (int)sdr1["ProdQty"] : 0;
                quotationDetails.VatAmt = 0;
                quotationDetails.VatPerc = "";
                quotationDetails1.Add(quotationDetails);
            }
            quotation.QuotationDetails = quotationDetails1;
            cnn.Close();
            return quotation;
        }

        public Quotation getQuotationdetails(int id)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            Quotation quotation = new Quotation();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"Quotations\" WHERE  \"Id\" = '" + id + "' ", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                quotation.Id = sdr0["Id"] != DBNull.Value ? (int)sdr0["Id"] : 0;
                quotation.CreatedOn = sdr0["CreatedOn"] != DBNull.Value ? (DateTime)sdr0["CreatedOn"] : DateTime.Now;
                quotation.CreatedBy = sdr0["CreatedBy"] != DBNull.Value ? (int)sdr0["CreatedBy"] : 0;
                quotation.QuotationDate = sdr0["QuotationDate"] != DBNull.Value ? (DateTime)sdr0["QuotationDate"] : DateTime.Now;
                quotation.Status = sdr0["Status"] != DBNull.Value ? (string)sdr0["Status"] : "";
                quotation.Details = sdr0["Details"] != DBNull.Value ? (string)sdr0["Details"] : "";
            }
            cnn.Close();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM \"QuotationDetails\" WHERE  \"Quotation_ref\" = '" + id + "' ", cnn).ExecuteReader();
            List<QuotationDetails> quotationDetails1 = new List<QuotationDetails>();
            while (sdr1.Read())
            {
                QuotationDetails quotationDetails = new QuotationDetails();
                quotationDetails.Total = sdr1["Total"] != DBNull.Value ? (decimal)sdr1["Total"] : 0;
                quotationDetails.VatPerc = sdr1["VatPerc"] != DBNull.Value ? (string)sdr1["VatPerc"] : "";
                quotationDetails.VatAmt = sdr1["VatAmt"] != DBNull.Value ? (decimal)sdr1["VatAmt"] : 0;
                quotationDetails.StkDesc = sdr1["StkDesc"] != DBNull.Value ? (string)sdr1["StkDesc"] : "";
                quotationDetails.UnitPrice = sdr1["UnitPrice"] != DBNull.Value ? (decimal)sdr1["UnitPrice"] : 0;
                quotationDetails.ProdQty = sdr1["ProdQty"] != DBNull.Value ? (int)sdr1["ProdQty"] : 0;
                quotationDetails1.Add(quotationDetails);
            }
            quotation.QuotationDetails = quotationDetails1;
            cnn.Close();
            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr3 = new NpgsqlCommand("Select * From \"Licence\" ", cnn).ExecuteReader();
            if (sdr3.Read())
            {
                lic.CompanyName = (string)sdr3["CompanyName"];
                lic.CompanySlogan = (string)sdr3["CompanySlogan"];
                lic.CompanyPostal = (string)sdr3["CompanyPostal"];
                lic.CompanyContact = (string)sdr3["CompanyContact"];
                lic.CompanyVAT = (string)sdr3["CompanyVAT"];
                lic.PhysicalAddress = (string)sdr3["PhysicalAddress"];
                lic.CompanyLogo = (string)sdr3["CompanyLogo"];
            }
            cnn.Close();
            string img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "company_profile");
            string base64ImageRepresentation = "";
            if (!String.IsNullOrEmpty(lic.CompanyLogo))
            {
                string full_imgPath = Path.Combine(img_path, lic.CompanyLogo);
                byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                base64ImageRepresentation = Convert.ToBase64String(imageArray);
            }
            else
            {
                string full_imgPath = Path.Combine(img_path, "invoice_default.jpg");
                byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
                base64ImageRepresentation = Convert.ToBase64String(imageArray);
            }
            lic.CompanyLogo = base64ImageRepresentation;
            quotation.license = lic;
            return quotation;
        }

        public List<Inventory> GetInventoryList()
        {
            throw new NotImplementedException();
        }

        public string SaveImage(IFormFile myfile)
        {
            //upload image
            string img_path = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "inventory");
            string rand_imageName = System.Guid.NewGuid().ToString("D") + ".jpg";
            //set the image path
            string full_imgPath = Path.Combine(img_path, rand_imageName);
            try
            {
                //using (FileStream fs = new FileStream(img_path + "\\" + rand_imageName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                using (FileStream fs = new FileStream(img_path + "/" + rand_imageName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    myfile.CopyTo(fs);
                    fs.Flush();
                }
            }
            catch (Exception e)
            {
                img_path = "product_default.png";
            }
            return rand_imageName;
        }
    }
}
