using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Models.StockInv;

namespace pyme_finance_api.Models.ReusableCodes
{
    public class ManageWarehouseSummary
    {
        private dbconnection myDbconnection = new dbconnection();

        public bool warehouse_summary_addstock(string dbname, int inventory_id, int inventory_qty, int staff_id)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(dbname));

            //check last warehouse summary id
            int last_ws_id = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(ws_id) as sl From warehouse_summary LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                last_ws_id = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();

            //check inventory data
            Inventory inv = new Inventory();
            cnn.Open();
            NpgsqlDataReader sdr_inv = new NpgsqlCommand("Select \"Inventory\".*, warehouses.wh_code, warehouses.wh_desc" +
                " From \"Inventory\" LEFT JOIN warehouses On warehouses.wh_ref = \"Inventory\".\"WarehouseRef\" WHERE \"InvtId\" = " + inventory_id + "  ", cnn).ExecuteReader();
            if (sdr_inv.HasRows == false)
            {
                return false;
            }
            while (sdr_inv.Read())
            {
                inv.WarehouseRef = sdr_inv["WarehouseRef"] != DBNull.Value ? (string)sdr_inv["WarehouseRef"] : null;
                inv.InvtQty = sdr_inv["InvtQty"] != DBNull.Value ? (int)sdr_inv["InvtQty"] : 0;
                inv.InvtRef = sdr_inv["InvtRef"] != DBNull.Value ? (string)sdr_inv["InvtRef"] : null;
            }
            cnn.Close();

            //get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(staff_id, dbname);
            if (staff_branch == 0)
            {
                return false;
            }
            string insertQ = "INSERT INTO warehouse_summary (ws_id, prod_id, wh_ref, bincode, openstock, qty_issued, qty_received, qty_adjusted, qty_allocated, " +
                "rt_rct_qty, rt_issue_qty, qty_on_order, physical_qty, free_qty, min_stock_qty, max_stock_qty, modified_on,  ws_branch,  ws_date )" +
                " VALUES(" + (last_ws_id + 1) + ", " + inventory_id + ", '" + inv.WarehouseRef + "', '" + inv.InvtRef + "', " + 0 + " ," + 0 + "," + 0 + "," + 0 + "," + inventory_qty + ", " + 0 + "," + 0 + ", " + 0 + ", " + inv.InvtQty + ", " + 0 + "," + inventory_qty + ", " + inventory_qty + ", '" + DateTime.Now + "', " + staff_branch + ", '"+DateTime.Now+"' ); ";
            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, dbname);

            return myReq2;

        }

        public bool updateinvoicefrompurchase(string dbname,int inventory_id,int transacted_qty,string type)
        {
            bool myReq2 = false;
            string up_inv = "";


            if (type == "Returned")
            {
                up_inv = "UPDATE \"Inventory\" SET \"InvtQty\" = \"InvtQty\" - " + transacted_qty +
                     " WHERE \"InvtType\" = 'GOODS' AND \"InvtId\" = " + inventory_id + " ";
            }
            else
            {
                 up_inv = "UPDATE \"Inventory\" SET \"InvtQty\" = \"InvtQty\" + " + transacted_qty +
                         " WHERE \"InvtType\" = 'GOODS' AND \"InvtId\" = " + inventory_id + " ";

            }
         
                 myReq2 = myDbconnection.UpdateDelInsert(up_inv, dbname);

            return myReq2;
              
            
        }

        public bool warehouse_summary_sl_pl(string dbname, int inventory_id, int transacted_qty, int staff_id, string transaction_type)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(dbname));

            //check last warehouse summary id
            int last_ws_id = 0;
            cnn.Open();
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(ws_id) as sl From warehouse_summary LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                last_ws_id = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();

            //check last transaction 
            int _openingstock = 0;
           
            cnn.Open();
            NpgsqlDataReader sdrb = new NpgsqlCommand("Select * From warehouse_summary WHERE prod_id = "+inventory_id+" ORDER BY ws_id DESC LIMIT 1 ", cnn).ExecuteReader();
            while (sdrb.Read())
            {
                _openingstock = sdrb["physical_qty"] != DBNull.Value ? (int)sdrb["physical_qty"] : 0;
                
            }
            cnn.Close();

            
            //check inventory data
            Inventory inv = new Inventory();
            cnn.Open();
            NpgsqlDataReader sdr_inv = new NpgsqlCommand("Select \"Inventory\".*, warehouses.wh_code, warehouses.wh_desc From \"Inventory\" " +
                " LEFT JOIN warehouses On warehouses.wh_ref = \"Inventory\".\"WarehouseRef\"" +
                " WHERE \"InvtId\" = " + inventory_id + "  ", cnn).ExecuteReader();
            if (sdr_inv.HasRows == false)
            {
                return false;
            }
            while (sdr_inv.Read())
            {
                inv.WarehouseRef = sdr_inv["WarehouseRef"] != DBNull.Value ? (string)sdr_inv["WarehouseRef"] : null;
                inv.InvtQty = sdr_inv["InvtQty"] != DBNull.Value ? (int)sdr_inv["InvtQty"] : 0;
                inv.InvtRef = sdr_inv["InvtRef"] != DBNull.Value ? (string)sdr_inv["InvtRef"] : null;
                inv.InvtType = sdr_inv["InvtType"] != DBNull.Value ? (string)sdr_inv["InvtType"] : null;
            }
            cnn.Close();

                // get staff branch
            int staff_branch = myDbconnection.GetStaffBranch(staff_id, dbname);
            if (staff_branch == 0)
            {
                return false;
            }

            //check transaction type
            int allocated_qty = 0;
            int remaining_qty = 0;

            //instantiate query
            string insertQ = null;

            if (transaction_type == "Sale")
            {
                if (inv.InvtType == "GOODS")
                {
                    remaining_qty = inv.InvtQty ;
                    allocated_qty = System.Math.Abs(transacted_qty) * (-1);
                    inv.InvtQty = System.Math.Abs(inv.InvtQty) * (-1);
                }
                else
                {
                    allocated_qty = 0;
                    //allocated_qty = System.Math.Abs(transacted_qty) * (-1);
                    remaining_qty = 0;
                }

                //create Sales query
                                       insertQ = "INSERT INTO warehouse_summary (ws_id, prod_id, wh_ref, bincode, openstock, qty_issued, qty_received, qty_adjusted, qty_allocated, rt_rct_qty, rt_issue_qty, qty_on_order, " +
                "physical_qty, free_qty, min_stock_qty, max_stock_qty, modified_on,  ws_branch,  ws_date )" +
 "VALUES(" + (last_ws_id + 1) + ", " + inventory_id + ", '" + inv.WarehouseRef + "', '" + inv.InvtRef + "', " + _openingstock + " ," + transacted_qty + "," + 0 + "," + 0 + "," + allocated_qty + ", " + 0 + "," + 0 + ", " + 0 + ", " + remaining_qty + ", " + 0 + "," + transacted_qty + ", " + transacted_qty + ", '" + DateTime.Now + "', " + staff_branch + ", '" + DateTime.Now + "' ); ";

            }

            else if (transaction_type == "Purchase")
            {
                if (inv.InvtType == "GOODS")
                {
                    remaining_qty = inv.InvtQty+ transacted_qty;
                    allocated_qty = System.Math.Abs(transacted_qty) * (+1);
                    inv.InvtQty = System.Math.Abs(inv.InvtQty) * (+1);
                }
                else
                {
                    allocated_qty = 0;
                    //allocated_qty = System.Math.Abs(transacted_qty) * (-1);
                    remaining_qty = 0;
                }

                //create Sales query
                insertQ = "INSERT INTO warehouse_summary (ws_id, prod_id, wh_ref, bincode, openstock, qty_issued, qty_received, qty_adjusted, qty_allocated, rt_rct_qty, rt_issue_qty, qty_on_order, physical_qty, free_qty, min_stock_qty, max_stock_qty, modified_on,  ws_branch,  ws_date )" +
                    " VALUES(" + (last_ws_id + 1) + ", " + inventory_id + ", '" + inv.WarehouseRef + "', '" + inv.InvtRef + "', " + _openingstock + " ," + 0 + "," + transacted_qty + "," + 0 + "," + allocated_qty + ", " + 0 + "," + 0 + ", " + 0 + ", " + remaining_qty + ", " + 0 + "," + transacted_qty + ", " + transacted_qty + ", '" + DateTime.Now + "', " + staff_branch + ", '" + DateTime.Now + "' ); ";

            }
            else if (transaction_type == "Reversal")
            {

                if (inv.InvtType == "GOODS")
                {
                    remaining_qty = inv.InvtQty;
                    allocated_qty = System.Math.Abs(transacted_qty) * (+1);
                    inv.InvtQty = System.Math.Abs(inv.InvtQty) * (+1);
                }
                else
                {
                    allocated_qty = 0;
                    //allocated_qty = System.Math.Abs(transacted_qty) * (-1);
                    remaining_qty = 0;
                }

                //create Sales query
                insertQ = "INSERT INTO warehouse_summary (ws_id, prod_id, wh_ref, bincode, openstock, qty_issued, qty_received, qty_adjusted, qty_allocated, rt_rct_qty, rt_issue_qty, qty_on_order, physical_qty, free_qty, min_stock_qty, max_stock_qty, modified_on,  ws_branch,  ws_date ) VALUES(" + (last_ws_id + 1) + ", " + inventory_id + ", '" + inv.WarehouseRef + "', '" + inv.InvtRef + "', " + _openingstock + " ," + 0 + "," + 0 + "," + -(transacted_qty) + "," + allocated_qty + ", " + 0 + "," + 0 + ", " + 0 + ", " + remaining_qty + ", " + 0 + "," + transacted_qty + ", " + transacted_qty + ", '" + DateTime.Now + "', " + staff_branch + ", '" + DateTime.Now + "' ); ";

            }
            else
            {
                return false;
            }

            bool myReq2 = myDbconnection.UpdateDelInsert(insertQ, dbname);
            return myReq2;

        }

        public List<WarehouseSummary> GetAllWarehouseSummary(string db, int yearRef, int branchRef)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db));

            List<WarehouseSummary> whsummary = new List<WarehouseSummary>();
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM warehouse_summary WHERE date_part('year', ws_date) = "+yearRef+ " AND ws_branch = "+branchRef+"; ", cnn).ExecuteReader();
            while (sdr0.Read())
            {
                WarehouseSummary ws = new WarehouseSummary();

                ws.ws_id = sdr0["ws_id"] != DBNull.Value ? (int)sdr0["ws_id"] : 0;
                ws.prod_id = sdr0["prod_id"] != DBNull.Value ? (int)sdr0["prod_id"] : 0;
                ws.wh_ref = sdr0["wh_ref"] != DBNull.Value ? (string)sdr0["wh_ref"] : null;
                ws.bincode = sdr0["bincode"] != DBNull.Value ? (string)sdr0["bincode"] : null;
                ws.openstock = sdr0["openstock"] != DBNull.Value ? (int)sdr0["openstock"] : 0;
                ws.qty_issued = sdr0["qty_issued"] != DBNull.Value ? (int)sdr0["qty_issued"] : 0;
                ws.qty_received = sdr0["qty_received"] != DBNull.Value ? (int)sdr0["qty_received"] : 0;
                ws.qty_adjusted = sdr0["qty_adjusted"] != DBNull.Value ? (int)sdr0["qty_adjusted"] : 0;
                ws.qty_allocated = sdr0["qty_allocated"] != DBNull.Value ? (int)sdr0["qty_allocated"] : 0;
                ws.rt_rct_qty = sdr0["rt_rct_qty"] != DBNull.Value ? (int)sdr0["rt_rct_qty"] : 0;
                ws.rt_issue_qty = sdr0["rt_issue_qty"] != DBNull.Value ? (int)sdr0["rt_issue_qty"] : 0;
                ws.qty_on_order = sdr0["qty_on_order"] != DBNull.Value ? (int)sdr0["qty_on_order"] : 0;
                ws.physical_qty = sdr0["physical_qty"] != DBNull.Value ? (int)sdr0["physical_qty"] : 0;
                ws.free_qty = sdr0["free_qty"] != DBNull.Value ? (int)sdr0["free_qty"] : 0;
                ws.min_stock_qty = sdr0["min_stock_qty"] != DBNull.Value ? (int)sdr0["min_stock_qty"] : 0;
                ws.max_stock_qty = sdr0["max_stock_qty"] != DBNull.Value ? (int)sdr0["max_stock_qty"] : 0;
                ws.modified_on = sdr0["modified_on"] != DBNull.Value ? (DateTime)sdr0["modified_on"] : DateTime.Now;
                ws.ws_branch = sdr0["ws_branch"] != DBNull.Value ? (int)sdr0["ws_branch"] : 0;
                ws.ws_date = sdr0["ws_date"] != DBNull.Value ? (DateTime)sdr0["ws_date"] : DateTime.Now;
                
                whsummary.Add(ws);
            }
            cnn.Close();

            return whsummary;

        }

        

    }
}
