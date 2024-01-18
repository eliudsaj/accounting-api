using Npgsql;
using pyme_finance_api.Common;
using pyme_finance_api.Models.DBConn;
using pyme_finance_api.Models.UnitofMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Service.MeasureofUnitService
{



    public interface IMeasureofUnitService
    {
         MyResponse saveUnitofMeasure(UnitofMeasure unitofMeasure);
        MyResponse updateUnitofMeasure(UnitofMeasure unitofMeasure);

        List<UnitofMeasure> listofUnitofMeasure();

        MyResponse deactivateUnitofMeasure();




    }
    public class MeasureofUnitService : IMeasureofUnitService
    {

        dbconnection myDbconnection = new dbconnection();

        public string OrganizationId { get; set; }

        public MeasureofUnitService(string organizationId)
        {
            OrganizationId = organizationId;
        }


        public MyResponse deactivateUnitofMeasure()
        {
            throw new NotImplementedException();
        }

        public List<UnitofMeasure> listofUnitofMeasure()
        {

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));


            cnn.Open();

            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM \"UnitofMeasure\" ", cnn).ExecuteReader();
            List<UnitofMeasure> unitofMeasures = new List<UnitofMeasure>();
          
            while (sdr0.Read())
            {
                UnitofMeasure unitofMeasure = new UnitofMeasure();
                unitofMeasure.CreatedOn = sdr0["created_on"] != DBNull.Value ? (DateTime)sdr0["created_on"] : DateTime.Today;
                unitofMeasure.Id = sdr0["id"] != DBNull.Value ? (int)sdr0["id"] : 0;
                unitofMeasure.Name = sdr0["name"] != DBNull.Value ? (string)sdr0["name"] : null;
                unitofMeasure.Status = sdr0["status"] != DBNull.Value ? (string)sdr0["status"] : null;
                unitofMeasures.Add(unitofMeasure);
            }
            cnn.Close();
            return unitofMeasures;
        }

        public MyResponse saveUnitofMeasure(UnitofMeasure unitofMeasure)
        {


            MyResponse response = new MyResponse();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
           cnn.Open();
            bool myReq1 = false;
            int groupnamecount = 0;

            ////check if group name exists
            NpgsqlDataReader sdr1 = new NpgsqlCommand("SELECT * FROM  \"UnitofMeasure\"   WHERE  \"name\"=  '" + unitofMeasure.Name + "' ;", cnn).ExecuteReader();
            while (sdr1.Read())
            {
                groupnamecount++;
            }
            if (groupnamecount >= 1)
            {
                response.Httpcode = 400;
                response.Message = "This  unit already exists in this branch";
              cnn.Close();
                return response;

            }
            else
            {

                unitofMeasure.Status = "ACTIVE";
                string insertQuery1 = "INSERT INTO \"UnitofMeasure\" (\"branch_id\",\"name\",\"created_on\",\"created_by\",\"status\") " +
                "VALUES('" + unitofMeasure.BranchId + "','" + unitofMeasure.Name + "','" + DateTime.Now + "', '" + unitofMeasure.CreatedBy + "', '" + unitofMeasure.Status + "' ) RETURNING \"id\" ;";

                myReq1 = myDbconnection.UpdateDelInsert(insertQuery1, OrganizationId);
                cnn.Close();
            }

            cnn.Open();

            //get last category id
            int last_cat_ID = 0;
          
            NpgsqlDataReader sdra = new NpgsqlCommand("Select MAX(id) as sl From \"UnitofMeasure\" LIMIT 1 ", cnn).ExecuteReader();
            while (sdra.Read())
            {
                last_cat_ID = sdra["sl"] != DBNull.Value ? (int)sdra["sl"] : 0;
            }
            cnn.Close();



            //     var cmd =   new NpgsqlCommand(insertQuery1, cnn,null);



            cnn.Close();

            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            response.Httpcode = 200;
            response.Message = last_cat_ID.ToString();



            return response;

    
        }

        public MyResponse updateUnitofMeasure(UnitofMeasure unitofMeasure)
        {

            MyResponse response = new MyResponse();
            string updtQ = "UPDATE \"UnitofMeasure\" SET \"name\" = '" + unitofMeasure.Name + "',\"modified_on\"='" + DateTime.Now + "' ";

            bool myReq1 = myDbconnection.UpdateDelInsert(updtQ, OrganizationId);

            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            return response;

        }
    }
}
