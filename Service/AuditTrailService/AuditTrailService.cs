using Npgsql;
using pyme_finance_api.Models.AuditTrail;
using pyme_finance_api.Models.DBConn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Service.AuditTrailService
{
    public class AuditTrailService
    {

        dbconnection myDbconnection = new dbconnection();
        public string OrganizationId { get; set; }


        public AuditTrailService(string organizationId )
        {
            OrganizationId = organizationId;


        }

        public void createAuditTrail(AuditTrail auditTrail) {
   
            string query = "INSERT INTO \"AuditTrail\" ( \"userid\",\"module\",\"action\",\"createdon\")VALUES('" + auditTrail.userId + "','" + auditTrail.module + "','" + auditTrail.action + "','"+DateTime.Now+"') ";
            bool myReq1 = myDbconnection.UpdateDelInsert(query, OrganizationId);
         
        }

        public List<AuditTrail> GetAllAuditTrails()
        {
            NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            List<AuditTrail> auditTrails = new List<AuditTrail>();
            cnn1.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM  \"AuditTrail\" ", cnn1).ExecuteReader();

            while (sdr0.Read())
            {
                AuditTrail auditTrail = new AuditTrail();
                //  string max = (string)sdr0["sj"];
             auditTrail.Id= sdr0["id"] != DBNull.Value ? (int)sdr0["id"] :0;
                auditTrail.userId = sdr0["userid"] != DBNull.Value ? (int)sdr0["userid"] : 0;
                auditTrail.module = sdr0["module"] != DBNull.Value ? (string)sdr0["module"] : "";
                auditTrail.action = sdr0["action"] != DBNull.Value ? (string)sdr0["action"] : "";
                auditTrail.createdOn = (DateTime)sdr0["createdon"];
                auditTrails.Add(auditTrail);

            }
            cnn1.Close();



            return auditTrails;
        }

   

        public List<AuditTrail> GetUserAuditTrails(int id)
        {
            NpgsqlConnection cnn1 = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            List<AuditTrail> auditTrails = new List<AuditTrail>();
            cnn1.Open();
            NpgsqlDataReader sdr0 = new NpgsqlCommand("SELECT * FROM  \"AuditTrail\" WHERE \"userid\" ="+id, cnn1).ExecuteReader();

            while (sdr0.Read())
            {
                AuditTrail auditTrail = new AuditTrail();
                //  string max = (string)sdr0["sj"];
                auditTrail.Id = sdr0["id"] != DBNull.Value ? (int)sdr0["id"] : 0;
                auditTrail.userId = sdr0["userid"] != DBNull.Value ? (int)sdr0["userid"] : 0;
                auditTrail.module = sdr0["module"] != DBNull.Value ? (string)sdr0["module"] : "";
                auditTrail.action = sdr0["action"] != DBNull.Value ? (string)sdr0["action"] : "";
                auditTrail.createdOn = (DateTime)sdr0["createdon"];
                auditTrails.Add(auditTrail);

            }
            cnn1.Close();
            return auditTrails;
        }
    }
}
