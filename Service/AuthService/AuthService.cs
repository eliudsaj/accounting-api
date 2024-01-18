using Npgsql;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.DBConn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Service.AuthService
{


    public interface IAuthService
    {
        bool Create_New_Database(string dbName,string ConnectionString);

        public License getCurrentCompany(string db);
    }
    public class AuthService : IAuthService
    {
        dbconnection myDbconnection = new dbconnection();

        public bool Create_New_Database(string dbName, string ConnectionString)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(ConnectionString);
            string qr = "CREATE DATABASE " + dbName.Trim().ToLower() + " WITH OWNER = postgres ENCODING = 'UTF8' ";
            //string qr = "CREATE DATABASE " + dbName.Trim().ToLower() + " WITH OWNER = postgres ";
            cnn.Open();
            bool success = new NpgsqlCommand(qr, cnn).ExecuteNonQuery() > 0;
            cnn.Close();

            ////check if success
            //if (success == false)
            //{
            //    return success;
            //}

           


            if (File.Exists(@"./SQLResources/clientExport2.sql"))
            {
                Console.WriteLine("File exists.");

            }
            else
            {
                Console.WriteLine("File doesnt exist");
            }


            NpgsqlConnection cnn2 = new NpgsqlConnection(new dbconnection().CheckConn(dbName.Trim().ToLower()));

            cnn2.Open();
            using var cmd = new NpgsqlCommand();
            try {

                cmd.Connection = cnn2;
                string myquery = System.IO.File.ReadAllText(@"./SQLResources/clientExport2.sql");
                cmd.CommandText = myquery;
                cmd.ExecuteNonQuery();
                cnn2.Dispose();
                cnn2.Close();
                return true;

            }
            catch (Exception E)
            {
                cnn2.Dispose();
                cnn2.Close();
                Console.WriteLine(E.Message);

            }

            return false;
      




        }

        public License getCurrentCompany(string db)
        {
            NpgsqlConnection cnn = new NpgsqlConnection(db);

            //get company data
            License lic = new License();
            cnn.Open();
            NpgsqlDataReader sdr1 = new NpgsqlCommand("Select * From \"Licence\" LIMIT 1 ", cnn).ExecuteReader();
            if (sdr1.Read())
            {
                lic.CompanyName = (string)sdr1["CompanyName"];
                lic.CompanySlogan = (string)sdr1["CompanySlogan"];
                lic.CompanyPostal = (string)sdr1["CompanyPostal"];
                lic.CompanyContact = (string)sdr1["CompanyContact"];
                lic.CompanyVAT = (string)sdr1["CompanyVAT"];
                lic.PhysicalAddress = (string)sdr1["PhysicalAddress"];
                lic.CompanyLogo = (string)sdr1["CompanyLogo"];
            }
            cnn.Close();



            return lic;

        }

      
    }
}
