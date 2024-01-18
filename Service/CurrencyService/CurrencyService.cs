using Npgsql;
using Org.BouncyCastle.Ocsp;
using pyme_finance_api.Common;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Service.CurrencyService
{

    public interface ICurrencyService
    {
        MyResponse AddCurrency(Currencies currencyData);

        MyResponse UpdateCurrency(Currencies currencyData);

        List<Currencies> GetCurrency();




    }




    public class CurrencyService : ICurrencyService
    {
        dbconnection myDbconnection = new dbconnection();

        public string OrganizationId { get; set; }


        public CurrencyService(string organizationId)
        {
            OrganizationId = organizationId;
        }




        public MyResponse AddCurrency(Currencies currencyData)
        {

            MyResponse response = new MyResponse();



            NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            int count = 0;


            cnn1.Open();

            string status = "Inactive";

            string insertQuery1 = "INSERT INTO \"Currencies\" (\"CrName\",\"CrCode\",\"CrCountry\",\"CrStatus\",\"CrCreatedDate\",\"CrModifiedDate\") " +
            "VALUES('" + currencyData.CrName + "','" + currencyData.CrCode + "','" + currencyData.CrCountry + "', '" + status + "', '" + DateTime.UtcNow.Date + "', '" + DateTime.UtcNow.Date + "' );";
            bool myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, OrganizationId);
            cnn1.Close();
            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            return response;
        }
        public List<Currencies> GetCurrency()
        {
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));



            //Get all currencies
            List<Currencies> currencyList = new List<Currencies>();
            cnn.Open();
            NpgsqlDataReader sdr_cr = new NpgsqlCommand("SELECT * FROM \"Currencies\" ", cnn).ExecuteReader();
            while (sdr_cr.Read())
            {
                Currencies cr = new Currencies();
                cr.CrId = sdr_cr["CrId"] != DBNull.Value ? (int)sdr_cr["CrId"] : 0;
                cr.CrName = sdr_cr["CrName"] != DBNull.Value ? (string)sdr_cr["CrName"] : null;
                cr.CrCode = sdr_cr["CrCode"] != DBNull.Value ? (string)sdr_cr["CrCode"] : null;
                cr.CrCountry = sdr_cr["CrCountry"] != DBNull.Value ? (string)sdr_cr["CrCountry"] : null;
                cr.CrStatus = sdr_cr["CrStatus"] != DBNull.Value ? (string)sdr_cr["CrStatus"] : null;
                cr.CrCreatedDate = sdr_cr["CrCreatedDate"] != DBNull.Value ? (DateTime)sdr_cr["CrCreatedDate"] : DateTime.Now;
                cr.CrModifiedDate = sdr_cr["CrModifiedDate"] != DBNull.Value ? (DateTime)sdr_cr["CrModifiedDate"] : DateTime.Now;
                currencyList.Add(cr);
            }
            cnn.Close();
            return currencyList;
        }

     
        MyResponse ICurrencyService.UpdateCurrency(Currencies currencyData)
        {
            throw new NotImplementedException();
        }
    }
}
