using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Org.BouncyCastle.Ocsp;
using pyme_finance_api.Common;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.GoodReceivedNote;
using pyme_finance_api.Models.NL.NlAccountGroup;
using pyme_finance_api.Models.ReusableCodes;
using pyme_finance_api.Models.Sales.InvoiceConfig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace pyme_finance_api.Service.GoodsReceivedNoteService
{
    public interface IGoodsReceiveNoteService
    {
        MyResponse addGoodReceiptNote(GoodsReceivedNote goodsReceivedNote);
        MyResponse addGoodReturnedNote(GoodsReceivedNote goodsReceivedNote);
        GoodsReceivedNote getGoodReceiptNoteDetail(int id);
        License GettingCompanyDetail();
        GoodsReceivedNote getGoodReturnNoteDetail(int id);
        GoodsReceivedNote getGoodReturnedNoteDetail(int id);
        List<GoodsReceivedNote> getAllGoodsReturnedNote();
        List<GoodsReceivedNote> getAllGoodsReceiptNote();
        List<GoodReturnNoteType> GetAllGoodReturnNoteType();
        List<PurchaseHeaderSettings> GetPurchaseHeaderSettings();
    }
    public class GoodsReceivedNoteService : IGoodsReceiveNoteService
    {
        dbconnection myDbconnection = new dbconnection();
        public string OrganizationId { get; set; }
        public GoodsReceivedNoteService(string organizationId)
        {
            OrganizationId = organizationId;
        }
        public List<GoodsReceivedNote> getAllGoodsReturnedNote()
        {
            string query = "SELECT a.\"id\" as Id,CONCAT(c.\"UFirstName\",c.\"ULastName\") as UserName,b.\"CustName\" as Supplier,a.invoice_id,a.\"received_date\",a.\"created_on\",a.\"additional_details\" as Details " +
                "FROM  \"goods_returned_note_header\" a " +
                "LEFT JOIN \"PLCustomer\" b on b.\"CustID\" = a.\"supplier_id\"  " +
                "LEFT JOIN \"Users\" c on c.\"UId\" = a.\"created_by\" ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            //get nlaccountsGroups
            List<GoodsReceivedNote> goodsreceivednotes = new List<GoodsReceivedNote>();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                GoodsReceivedNote goodsReceivedNote = new GoodsReceivedNote();
                goodsReceivedNote.createdOn = (DateTime)sdr0["created_on"];
                goodsReceivedNote.ReceivedDate = (DateTime)sdr0["received_date"];
                goodsReceivedNote.invoiceId = sdr0["invoice_id"] != DBNull.Value ? (string)sdr0["invoice_id"] : null;
                goodsReceivedNote.id = sdr0["Id"] != DBNull.Value ? (int)sdr0["Id"] : 0;
                goodsReceivedNote.username = sdr0["UserName"] != DBNull.Value ? (string)sdr0["UserName"] : null;
                goodsReceivedNote.supplier = sdr0["Supplier"] != DBNull.Value ? (string)sdr0["Supplier"] : null;
                goodsReceivedNote.Details = sdr0["Details"] != DBNull.Value ? (string)sdr0["Details"] : null;
                goodsreceivednotes.Add(goodsReceivedNote);
            }
            cnn.Dispose();
            cnn.Close();
            return goodsreceivednotes;
        }
        public List<GoodsReceivedNote> getAllGoodsReceiptNote()
        {
            string query = "SELECT a.\"id\" as Id,CONCAT(c.\"UFirstName\",c.\"ULastName\") as UserName,b.\"CustName\" as Supplier,a.invoice_id,a.\"received_date\",a.\"created_on\",a.\"additional_details\" as Details " +
                "FROM  \"goods_received_note_header\" a " +
                "LEFT JOIN \"PLCustomer\" b on b.\"CustID\" = a.\"supplier_id\"  " +
                "LEFT JOIN \"Users\" c on c.\"UId\" = a.\"created_by\" ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));

            //get nlaccountsGroups
            List<GoodsReceivedNote> goodsreceivednotes = new List<GoodsReceivedNote>();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {
                GoodsReceivedNote goodsReceivedNote = new GoodsReceivedNote();
                goodsReceivedNote.createdOn = (DateTime)sdr0["created_on"];
                goodsReceivedNote.ReceivedDate = (DateTime)sdr0["received_date"];
                goodsReceivedNote.invoiceId = sdr0["invoice_id"] != DBNull.Value ? (string)sdr0["invoice_id"] : null;
                goodsReceivedNote.id = sdr0["Id"] != DBNull.Value ? (int)sdr0["Id"] : 0;
                goodsReceivedNote.username = sdr0["UserName"] != DBNull.Value ? (string)sdr0["UserName"] : null;
                goodsReceivedNote.supplier = sdr0["Supplier"] != DBNull.Value ? (string)sdr0["Supplier"] : null;
                goodsReceivedNote.Details = sdr0["Details"] != DBNull.Value ? (string)sdr0["Details"] : null;
                goodsreceivednotes.Add(goodsReceivedNote);
            }
            cnn.Close();
            return goodsreceivednotes;
        }
        public GoodsReceivedNote getGoodReturnedNoteDetail(int id)
        {
            try
            {
                string query = "SELECT a.\"id\" as Id,CONCAT(c.\"UFirstName\",c.\"ULastName\") as UserName,b.\"CustName\" as Supplier,a.\"received_date\",a.\"created_on\",a.\"additional_details\" as Details,a.invoice_id " +
                    "FROM  \"goods_returned_note_header\" a " +
                    "LEFT JOIN \"PLCustomer\" b on b.\"CustID\" = a.\"supplier_id\"  " +
                    "LEFT JOIN \"Users\" c on c.\"UId\" = a.\"created_by\"" +
                    $"WHERE  a.\"id\" = {id}";

                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
                //get nlaccountsGroups
                List<GoodsReceivedNote> goodsreceivednotes = new List<GoodsReceivedNote>();
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    GoodsReceivedNote goodsReceivedNote = new GoodsReceivedNote();
                    goodsReceivedNote.createdOn = (DateTime)sdr0["created_on"];
                    goodsReceivedNote.ReceivedDate = (DateTime)sdr0["received_date"];
                    goodsReceivedNote.id = sdr0["Id"] != DBNull.Value ? (int)sdr0["Id"] : 0;
                    goodsReceivedNote.invoiceId = sdr0["invoice_id"] != DBNull.Value ? (string)sdr0["invoice_id"] : null;
                    goodsReceivedNote.username = sdr0["UserName"] != DBNull.Value ? (string)sdr0["UserName"] : null;
                    goodsReceivedNote.supplier = sdr0["Supplier"] != DBNull.Value ? (string)sdr0["Supplier"] : null;
                    goodsReceivedNote.Details = sdr0["Details"] != DBNull.Value ? (string)sdr0["Details"] : null;
                    goodsreceivednotes.Add(goodsReceivedNote);
                }
                cnn.Close();
                cnn.Open();
                string query2 = "SELECT a.\"product_id\",a.\"quantity\" as ProductAmount ,b.\"InvtName\" as Product" +
                    " FROM  \"goods_returned_note_details\" a" +
                    " LEFT JOIN \"Inventory\" b ON b.\"InvtId\" = a.\"product_id\" WHERE a.header_id ='" + id + "';";
                List<GoodReceivedNoteProductResponse> productResponses = new List<GoodReceivedNoteProductResponse>();
                var data = goodsreceivednotes.FirstOrDefault();
                NpgsqlDataReader sdr1 = new NpgsqlCommand(query2, cnn).ExecuteReader();
                while (sdr1.Read())
                {
                    GoodReceivedNoteProductResponse goodReceivedNoteProductResponse = new GoodReceivedNoteProductResponse();
                    goodReceivedNoteProductResponse.Product = sdr1["Product"] != DBNull.Value ? (string)sdr1["Product"] : "";
                    goodReceivedNoteProductResponse.ProductAmount = sdr1["ProductAmount"] != DBNull.Value ? (int)sdr1["ProductAmount"] : 0;
                    productResponses.Add(goodReceivedNoteProductResponse);
                }
                data.goodReceivedNoteProductResponses = productResponses;
                cnn.Close();
                return data;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public List<GoodReturnNoteType> GetAllGoodReturnNoteType()
        {
            string query = "SELECT * FROM \"GoodReturnNoteType\" ORDER BY \"GRNId\" ASC ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<GoodReturnNoteType> goodReturn = new List<GoodReturnNoteType>();
            NpgsqlDataReader reader = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (reader.Read())
            {
                GoodReturnNoteType goodReturnNote = new GoodReturnNoteType();
                goodReturnNote.GRNId = reader["GRNId"] != DBNull.Value ? (int)reader["GRNId"] : 0;
                goodReturnNote.GRNType = reader["GRNType"] != DBNull.Value ? (string)reader["GRNType"] : null;
                goodReturnNote.GRNComment = reader["GRNComment"] != DBNull.Value ? (string)reader["GRNComment"] : null;
                goodReturn.Add(goodReturnNote);
            }
            cnn.Close();
            return goodReturn;
        }
        public List<PurchaseHeaderSettings> GetPurchaseHeaderSettings()
        {
            List<PurchaseHeaderSettings> headerSettings = new List<PurchaseHeaderSettings>();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            NpgsqlDataReader reader1 = new NpgsqlCommand("SELECT * FROM \"Document_header\" WHERE \"Category\" = 'PURCHASE' ORDER BY id ASC; ", cnn).ExecuteReader();
            while (reader1.Read())
            {
                PurchaseHeaderSettings head = new PurchaseHeaderSettings();
                head.Id = reader1["id"] != DBNull.Value ? (int)reader1["id"] : 0;
                head.Category = reader1["Category"] != DBNull.Value ? (string)reader1["Category"] : null;
                head.DocumentName = reader1["DocumentName"] != DBNull.Value ? (string)reader1["DocumentName"] : null;
                head.Status = reader1["Status"] != DBNull.Value ? (bool)reader1["Status"] : false;
                headerSettings.Add(head);
            }
            cnn.Close();
            return headerSettings;
        }
        public GoodsReceivedNote getGoodReturnNoteDetail(int id)
        {
            try
            {
                string query = "SELECT a.\"id\" as Id,CONCAT(c.\"UFirstName\",c.\"ULastName\") as UserName,b.\"CustName\" as Supplier, b.\"PLCustCode\", b.\"PostalAddress\", b.\"PhysicalAddress\", b.\"VATNo\",a.\"received_date\",a.\"created_on\",a.\"additional_details\" as Details,a.invoice_id " +
                    "FROM  \"goods_returned_note_header\" a " +
                    "LEFT JOIN \"PLCustomer\" b on b.\"CustID\" = a.\"supplier_id\"  " +
                    "LEFT JOIN \"Users\" c on c.\"UId\" = a.\"created_by\"" +
                    $"WHERE  a.\"id\" = {id}";
                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
                //get nlaccountsGroups
                List<GoodsReceivedNote> goodsreceivednotes = new List<GoodsReceivedNote>();
                cnn.Open();
                NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
                while (sdr0.Read())
                {
                    GoodsReceivedNote goodsReceivedNote = new GoodsReceivedNote();
                    goodsReceivedNote.createdOn = (DateTime)sdr0["created_on"];
                    goodsReceivedNote.ReceivedDate = (DateTime)sdr0["received_date"];
                    goodsReceivedNote.id = sdr0["Id"] != DBNull.Value ? (int)sdr0["Id"] : 0;
                    goodsReceivedNote.invoiceId = sdr0["invoice_id"] != DBNull.Value ? (string)sdr0["invoice_id"] : null;
                    goodsReceivedNote.username = sdr0["UserName"] != DBNull.Value ? (string)sdr0["UserName"] : null;
                    goodsReceivedNote.supplier = sdr0["Supplier"] != DBNull.Value ? (string)sdr0["Supplier"] : null;
                    goodsReceivedNote.SupplierCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null;
                    goodsReceivedNote.AddressContact = sdr0["PostalAddress"] != DBNull.Value ? (string)sdr0["PostalAddress"] : null;
                    goodsReceivedNote.PysicalAddress = sdr0["PhysicalAddress"] != DBNull.Value ? (string)sdr0["PhysicalAddress"] : null;
                    goodsReceivedNote.VatCode = sdr0["VATNo"] != DBNull.Value ? (string)sdr0["VATNo"] : null;
                    goodsReceivedNote.Details = sdr0["Details"] != DBNull.Value ? (string)sdr0["Details"] : null;
                    goodsreceivednotes.Add(goodsReceivedNote);
                }
                cnn.Close();
                cnn.Open();
                string query2 = "SELECT a.\"product_id\",a.\"quantity\" as ProductAmount ,b.\"InvtName\" as Product" +
                    " FROM  \"goods_returned_note_details\" a" +
                    " LEFT JOIN \"Inventory\" b ON b.\"InvtId\" = a.\"product_id\" WHERE a.header_id ='" + id + "';";
                List<GoodReceivedNoteProductResponse> productResponses = new List<GoodReceivedNoteProductResponse>();
                var data = goodsreceivednotes.FirstOrDefault();
                NpgsqlDataReader sdr1 = new NpgsqlCommand(query2, cnn).ExecuteReader();
                while (sdr1.Read())
                {
                    GoodReceivedNoteProductResponse goodReceivedNoteProductResponse = new GoodReceivedNoteProductResponse();
                    goodReceivedNoteProductResponse.Product = sdr1["Product"] != DBNull.Value ? (string)sdr1["Product"] : "";
                    goodReceivedNoteProductResponse.ProductAmount = sdr1["ProductAmount"] != DBNull.Value ? (int)sdr1["ProductAmount"] : 0;
                    productResponses.Add(goodReceivedNoteProductResponse);
                }
                data.goodReceivedNoteProductResponses = productResponses;
                cnn.Close();
                return data;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public License GettingCompanyDetail()
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            License lic = new License();
            cnn.Open();
            string query = "Select * From \"Licence\"  ";
            NpgsqlDataReader reader = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (reader.Read())
            {
                lic.CompanyName = (string)reader["CompanyName"];
                lic.CompanySlogan = (string)reader["CompanySlogan"];
                lic.CompanyPostal = (string)reader["CompanyPostal"];
                lic.CompanyContact = (string)reader["CompanyContact"];
                lic.CompanyVAT = (string)reader["CompanyVAT"];
                lic.PhysicalAddress = (string)reader["PhysicalAddress"];
                lic.CompanyLogo = (string)reader["CompanyLogo"];
            }
            cnn.Close();
            return lic;
        }
        public GoodsReceivedNote getGoodReceiptNoteDetail(int id)
        {
            try {
            string query = "SELECT a.\"id\" as Id,CONCAT(c.\"UFirstName\",c.\"ULastName\") as UserName,b.\"CustName\" as Supplier, b.\"PLCustCode\", b.\"PostalAddress\", b.\"PhysicalAddress\", b.\"VATNo\",a.\"received_date\"," +
                    "a.\"created_on\",a.\"additional_details\" as Details,a.invoice_id " +
                "FROM  \"goods_received_note_header\" a LEFT JOIN \"PLCustomer\" b on b.\"CustID\" = a.\"supplier_id\"  " +
                "LEFT JOIN \"Users\" c on c.\"UId\" = a.\"created_by\"" +
                $"WHERE  a.\"id\" = {id}";


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            //get nlaccountsGroups
            List<GoodsReceivedNote> goodsreceivednotes = new List<GoodsReceivedNote>();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();

            while (sdr0.Read())
            {
                GoodsReceivedNote goodsReceivedNote = new GoodsReceivedNote();
                goodsReceivedNote.createdOn = (DateTime)sdr0["created_on"];
                goodsReceivedNote.ReceivedDate = (DateTime)sdr0["received_date"];
                goodsReceivedNote.id = sdr0["Id"] != DBNull.Value ? (int)sdr0["Id"] : 0;
                goodsReceivedNote.invoiceId = sdr0["invoice_id"] != DBNull.Value ? (string)sdr0["invoice_id"] : null;
                goodsReceivedNote.username = sdr0["UserName"] != DBNull.Value ? (string)sdr0["UserName"] : null;
                goodsReceivedNote.supplier = sdr0["Supplier"] != DBNull.Value ? (string)sdr0["Supplier"] : null;
                goodsReceivedNote.SupplierCode = sdr0["PLCustCode"] != DBNull.Value ? (string)sdr0["PLCustCode"] : null;
                goodsReceivedNote.AddressContact = sdr0["PostalAddress"] != DBNull.Value ? (string)sdr0["PostalAddress"] : null;
                goodsReceivedNote.PysicalAddress = sdr0["PhysicalAddress"] != DBNull.Value ? (string)sdr0["PhysicalAddress"] : null;
                goodsReceivedNote.VatCode = sdr0["VATNo"] != DBNull.Value ? (string)sdr0["VATNo"] : null;
                goodsReceivedNote.Details = sdr0["Details"] != DBNull.Value ? (string)sdr0["Details"] : null;
                goodsreceivednotes.Add(goodsReceivedNote);
            }
            cnn.Close();
                cnn.Open();
                string query2 = "SELECT a.\"product_id\",a.\"quantity\" as ProductAmount ,b.\"InvtName\" as Product" +
                    " FROM  \"goods_received_note_details\" a" +
                    " LEFT JOIN \"Inventory\" b ON b.\"InvtId\" = a.\"product_id\" WHERE a.header_id ='"+id+"';";
            List<GoodReceivedNoteProductResponse> productResponses = new List<GoodReceivedNoteProductResponse>();
            var data = goodsreceivednotes.FirstOrDefault();
            NpgsqlDataReader sdr1 = new NpgsqlCommand(query2, cnn).ExecuteReader();
            while (sdr1.Read())
            {
                GoodReceivedNoteProductResponse goodReceivedNoteProductResponse = new GoodReceivedNoteProductResponse();
                goodReceivedNoteProductResponse.Product = sdr1["Product"] != DBNull.Value ? (string)sdr1["Product"] : "";
                goodReceivedNoteProductResponse.ProductAmount = sdr1["ProductAmount"] != DBNull.Value ? (int)sdr1["ProductAmount"] : 0;
                productResponses.Add(goodReceivedNoteProductResponse);
            }
            data.goodReceivedNoteProductResponses = productResponses;
                cnn.Close();
                return data;
            }
            catch (Exception e) {            
                throw e;
            }
        }
        public MyResponse addGoodReturnedNote(GoodsReceivedNote goodsReceivedNote)
        {

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            MyResponse response = new MyResponse();
            cnn.Open();
            int id = 0;

            string query = "INSERT INTO goods_returned_note_header (\"supplier_id\",\"received_date\",\"created_on\",\"created_by\",\"additional_details\",\"invoice_id\")" +
                "VALUES('" + goodsReceivedNote.supplierId + "','" + goodsReceivedNote.ReceivedDate + "','" + DateTime.Now + "','" + goodsReceivedNote.createdBy + "','" + goodsReceivedNote.Details + "','" + goodsReceivedNote.invoiceId + "')" +
                "RETURNING \"id\" ;";

            using (var trans = cnn.BeginTransaction())
            {

                try
                {
                    var cmd = new NpgsqlCommand(query, cnn, trans);
                    id = int.Parse(cmd.ExecuteScalar().ToString());
                    StringBuilder sb = new StringBuilder();
                    foreach (var data in goodsReceivedNote.goodReceivedNoteProducts)
                    {
                        sb.Append("INSERT INTO goods_returned_note_details (\"header_id\",\"product_id\",\"quantity\")" +
                            "VALUES('" + id + "','" + data.ProductId + "','" + data.ProductAmount + "');");
                    }
                    cmd.CommandText = sb.ToString();
                    //cmd.CommandText = creditentry2;
                    //cmd.CommandText = debitentry1;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    cnn.Close();
                }
                catch (Exception e)
                {

                    trans.Rollback();
                    response.Httpcode = 400;
                    Console.WriteLine(e.Message);
                    response.Message = e.Message;
                    cnn.CloseAsync();
                    return response;
                    throw e;
                }
            }
            ManageWarehouseSummary whs = new ManageWarehouseSummary();
            foreach (var data in goodsReceivedNote.goodReceivedNoteProducts)
            {
                bool wh_req = whs.warehouse_summary_sl_pl(OrganizationId, data.ProductId, data.ProductAmount, goodsReceivedNote.createdBy, "Sale");
                if (wh_req)
                {
                    bool inv_req = whs.updateinvoicefrompurchase(OrganizationId, data.ProductId, data.ProductAmount, "Returned");
                }
            }
            response.Httpcode = 200;
            response.Message = "success";
            return response;
        }
        public MyResponse addGoodReceiptNote(GoodsReceivedNote goodsReceivedNote)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            MyResponse response = new MyResponse();
            cnn.Open();
            int id = 0;
            string query = "INSERT INTO goods_received_note_header (\"supplier_id\",\"received_date\",\"created_on\",\"created_by\",\"additional_details\",\"invoice_id\")" +
                "VALUES('" + goodsReceivedNote.supplierId + "','" + goodsReceivedNote.ReceivedDate + "','" + DateTime.Now + "','" + goodsReceivedNote.createdBy + "','" + goodsReceivedNote.Details + "','" + goodsReceivedNote.invoiceId + "')" +
                "RETURNING \"id\" ;";

            using (var trans = cnn.BeginTransaction())
            {
                try
                {
                    var cmd = new NpgsqlCommand(query, cnn, trans);
                    id = int.Parse(cmd.ExecuteScalar().ToString());
                    StringBuilder sb = new StringBuilder();
                    foreach (var data in goodsReceivedNote.goodReceivedNoteProducts)
                    {
                        sb.Append("INSERT INTO goods_received_note_details (\"header_id\",\"product_id\",\"quantity\")" +
                            "VALUES('" + id + "','" + data.ProductId + "','" + data.ProductAmount + "');");
                    }
                    cmd.CommandText = sb.ToString();
                    //cmd.CommandText = creditentry2;
                    //cmd.CommandText = debitentry1;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    cnn.Close();
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    response.Httpcode = 400;
                    Console.WriteLine(e.Message);
                    response.Message = e.Message;
                    cnn.CloseAsync();
                    return response;
                    throw e;
                }
            }
            ManageWarehouseSummary whs = new ManageWarehouseSummary();
            foreach (var data in goodsReceivedNote.goodReceivedNoteProducts)
            {
                bool wh_req = whs.warehouse_summary_sl_pl(OrganizationId, data.ProductId, data.ProductAmount, goodsReceivedNote.createdBy, "Purchase");
                if (wh_req)
                {
                    bool inv_req = whs.updateinvoicefrompurchase(OrganizationId, data.ProductId, data.ProductAmount, "");
                }
            }
            response.Httpcode = 200;
            response.Message = "success";
            return response;
        }        
    }
}







