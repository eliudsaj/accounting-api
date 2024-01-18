using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using pyme_finance_api.Common;

namespace pyme_finance_api.Models.DBConn
{
    public class dbconnection
    {
        private static Random random = new Random();
        public static string DbUrl = ConfigHelper.AppSetting("DbUrl");
        public static string UserId = ConfigHelper.AppSetting("DbUserName");
        public static string Password = ConfigHelper.AppSetting("DbPass");
        public string CheckConn(string db)
        {
            //string conn = $"host=localhost; port=5432; Database={db}; user id=postgres; password=justoh; Integrated Security=true; Pooling=true;";
            string conn = $"host={DbUrl}; port=5432; Database={db.ToLower()}; user id={UserId}; password={Password}; Integrated Security=true; Pooling=true;";
            return conn;
        }

        public DataTable Processor(string qr, string db)
        {
            DataTable dataTable = new DataTable();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db.ToLower()));
            cnn.Open();
            new NpgsqlDataAdapter("" + qr + "", cnn).Fill(dataTable);
            cnn.Close();
            return dataTable;
        }

        public bool UpdateDelInsert(string qr, string db)
        {
            try
            {
                NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db.ToLower()));
                cnn.Open();
                bool success = new NpgsqlCommand(qr, cnn).ExecuteNonQuery() > 0;
                cnn.Dispose();
                cnn.Close();
                return success;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public int CheckRowExists(string qr, string db)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db.ToLower()));
            cnn.Open();
            NpgsqlCommand cmd = new NpgsqlCommand(qr, cnn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
            DataSet ds1 = new DataSet();
            da.Fill(ds1);
            int i = ds1.Tables[0].Rows.Count;
            cnn.Close();
            return i;
        }

        public int GetStaffBranch(int staff_id, string db)
        {
            int StaffBranch = 0;
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(db.ToLower()));
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("Select * From \"Users\" WHERE \"UId\" = " + staff_id + "  ", cnn).ExecuteReader();
            if (sdr0.HasRows == false)
            {
                StaffBranch = 0;
            }
            if (sdr0.Read())
            {
                StaffBranch = sdr0["UBranch"] != DBNull.Value ? (int)sdr0["UBranch"] : 0;
            }
            cnn.Close();
            return StaffBranch;
        }

        public string Generate_RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public string Generate_RandomInt(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public bool CheckRights(string dbName, string colName, int userRef)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(dbName.ToLower()));
            bool hasPermission = false;
            cnn.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"UserPermissions\" WHERE \"" + colName + "\" = 'true' AND \"PUser\" = " + userRef + "  ", cnn).ExecuteReader();
            if (sdr0.HasRows == true)
            {
                hasPermission = true;
            }
            else
            {
                hasPermission = false;
            }
            cnn.Close();
            return hasPermission;
        }
    }
}
