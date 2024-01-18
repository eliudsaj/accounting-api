using Npgsql;
using pyme_finance_api.Common;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.DBConn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pyme_finance_api.Service.UserService
{

    public interface IUserService
    {
        MyResponse GetUserPrimaryDetail(int userId);

        MyResponse UpdateUserPrimaryDetail(Users users);

        MyResponse AddPermissionToGroup(AddPermissionToGroup addPermissionToGroup);

        MyResponse RemovePermissionFromGroup(GroupPermmissions groupPermmissions);

        public List<Group> getGroups();

        public List<Group> getUserGroups(int userId);

        public List<GroupPermmissions> groupPermmissions();

        public List<UserGroups> getGroupMembers(int groupId);

        public MyResponse addUserGroup(AddGroupRequest addGroupRequest, int userId);


        public MyResponse deactivategroup(int groupId);

        public MyResponse activategroup(int groupId);


        public void getGroupPermissions(int groupId);




    }








    public class UserService : IUserService
    {

        dbconnection myDbconnection = new dbconnection();

        public string OrganizationId { get; set; }


        public UserService(string organizationId)
        {

            OrganizationId = organizationId;
        }




        public List<Group> getGroups()
        {

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            string query = "SELECT * FROM \"Groups\" ";
            List<Group> groups = new List<Group>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();

            while (sdr0.Read())
            {
              Group group = new Group();
             
                group.Status = sdr0["status"] != DBNull.Value ? (string)sdr0["status"] : null;
                group.CreatedBy = sdr0["created_by"] != DBNull.Value ? (int)sdr0["created_by"] : 0;
                group.CreatedOn =  (DateTime)sdr0["created_on"] ;
                group.Name = sdr0["name"] != DBNull.Value ? (string)sdr0["name"] : null;
                group.Id = (int)sdr0["id"]; ;
                groups.Add(group);
            }

            cnn.Close();


            return groups;
        }

        public void getGroupPermissions(int groupId)
        {


            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            string query = "SELECT a.\"status\",a.\"created_by\",a.\"name\",a.\"id\",a.\"createdOn\" FROM \"UserGroupsPermission\" a " +
                "LEFT JOIN \"UserGroups\" b  ON  b.\"group_id\"  =  a.\"group_id\" WHERE  a.\"group_id\" = "+groupId+" ";
          
            List<string> permissions = new List<string>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();

            while (sdr0.Read())
            {

                string permission = (string)sdr0["name"];
                permissions.Add(permission);
            }

            cnn.Close();



            throw new NotImplementedException();
        }



        public List<string> getUserGroupPermission(int userId)
        {
            string query = "SELECT a.\"group_id\",a.\"permission\" FROM \"UserGroupsPermission\" a LEFT JOIN \"UserGroups\" b on b.\"group_id\" = a.\"group_id\" WHERE b.\"user_id\" = '"+userId+"';";

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            List<string> group_permission = new List<string>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();
            while (sdr0.Read())
            {

              string permission = sdr0["permission"] != DBNull.Value ? (string)sdr0["permission"] : null;
                group_permission.Add(permission);
            }
            cnn.Close();


            return group_permission;
        }



        public List<Group> getUserGroups(int userId)
        {

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            string query = "SELECT a.\"status\",a.\"created_by\",a.\"name\",a.\"id\",a.\"created_on\" FROM \"Groups\" a " +
                "LEFT JOIN \"UserGroups\" b  ON  b.\"group_id\"  =  a.\"id\" " +
                "LEFT JOIN \"Users\" c ON c.\"UId\"  =  b.\"user_id\" " +
                " WHERE c.\"UId\" = " + userId + " ";
            List<Group> groups = new List<Group>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();

            while (sdr0.Read())
            {
                Group group = new Group();

                group.Status = sdr0["status"] != DBNull.Value ? (string)sdr0["status"] : null;
                group.CreatedBy = sdr0["created_by"] != DBNull.Value ? (int)sdr0["created_by"] : 0;
                group.CreatedOn = (DateTime)sdr0["created_on"];
                group.Id = (int)sdr0["id"];
                group.Name = sdr0["name"] != DBNull.Value ? (string)sdr0["name"] : null;
                groups.Add(group);
            }

            cnn.Close();
           






            return groups;
        }

        public MyResponse GetUserPrimaryDetail(int userId)
        {
            throw new NotImplementedException();
        }

        public MyResponse UpdateUserPrimaryDetail(Users users)
        {
            throw new NotImplementedException();
        }

        public MyResponse deactivategroup(int groupId)
        {
            MyResponse response = new MyResponse();

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();

            string status = "INACTIVE";
            string updtQ = "UPDATE \"Groups\" SET \"status\" = '" + status + "' WHERE \"id\" = '" + groupId + "' ";


            bool myReq1 = myDbconnection.UpdateDelInsert(updtQ, OrganizationId);


            cnn.Close();

            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            else
            {
                response.Httpcode = 200;
                response.Message = "Group has been deactivated successfuly";
            }


            return response;
           
        }

        public MyResponse activategroup(int groupId)
        {

            MyResponse response = new MyResponse();

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();

            string status = "ACTIVE";
            string updtQ = "UPDATE \"Groups\" SET \"status\" = '" + status + "' WHERE \"id\" = '" + groupId + "' ";


            bool myReq1 = myDbconnection.UpdateDelInsert(updtQ, OrganizationId);


            cnn.Close();

            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            else
            {
                response.Httpcode = 200;
                response.Message = "Group has been activated successfuly";
            }



            return response;
        }

        public MyResponse addUserGroup(AddGroupRequest addGroupRequest,int userId)
        {
            MyResponse response = new MyResponse();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.OpenAsync();
            using (var trans = cnn.BeginTransaction())
            {

                try
                {
                    long insertedId = 0L;
                    string status = "ACTIVE";
                    string insertQuery1 = "INSERT INTO \"Groups\" (\"name\",\"created_on\",\"created_by\",\"status\") " +
                   "VALUES('" + addGroupRequest.Name + "','" + DateTime.Now + "','" + userId + "','"+status+"' ) RETURNING \"id\" ;";


                    var cmd = new NpgsqlCommand(insertQuery1, cnn, trans);

                    insertedId = int.Parse(cmd.ExecuteScalar().ToString());
                    StringBuilder sb = new StringBuilder();

                    foreach (int id in addGroupRequest.Users)
                    {
                        sb.Append("INSERT INTO  \"UserGroups\" (\"group_id\",\"user_id\") VALUES ('" + insertedId + "','" + id + "'); ");


                    }
                    foreach (string name in addGroupRequest.Permissions)
                    {
                        sb.Append("INSERT INTO  \"UserGroupsPermission\" (\"group_id\",\"permission\") VALUES ('" + insertedId + "','" + name + "'); ");

                    }
                    string query = sb.ToString();
                    cmd.CommandText = query;

                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    response.Httpcode = 200;
                    response.Message = "success";


                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    trans.Rollback();
                    response.Httpcode = 400;
                    response.Message = "An occured while trying to save details.";


                }


            }





            return response;





        }

        public List<GroupPermmissions> groupPermmissions()
        {

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            string query = "SELECT * FROM \"UserGroupsPermission\" ;";
            List<GroupPermmissions> groups = new List<GroupPermmissions>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();

            while (sdr0.Read())
            {
                GroupPermmissions group_permission = new GroupPermmissions();

                group_permission.Permission = sdr0["permission"] != DBNull.Value ? (string)sdr0["permission"] : null;
                group_permission.GroupId = sdr0["group_id"] != DBNull.Value ? (int)sdr0["group_id"] : 0;
         
                groups.Add(group_permission);
            }
            cnn.Close();

            return groups;
        }

        public List<UserGroups> getGroupMembers( int groupId)
        {

            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            string query = "SELECT * FROM \"UserGroups\" WHERE \"group_id\" = '"+groupId+"' ;";
            List<UserGroups> groups = new List<UserGroups>();
            NpgsqlDataReader sdr0 = new NpgsqlCommand(query, cnn).ExecuteReader();

            while (sdr0.Read())
            {
                UserGroups user_group = new UserGroups();

                user_group.UserId = sdr0["user_id"] != DBNull.Value ? (int)sdr0["user_id"] : 0;
                user_group.GroupId = sdr0["group_id"] != DBNull.Value ? (int)sdr0["group_id"] : 0;

                groups.Add(user_group);
            }
            cnn.Close();


            return groups;
        }



        public GroupData  GetGroupData(int GroupId)
        {
            GroupData groupData = new GroupData();
            groupData.Name = getGroups().Where(x => x.Id == GroupId).FirstOrDefault().Name;
            groupData.GroupPermmissions = groupPermmissions().Where(x=> x.GroupId == GroupId).ToList();
            groupData.UserGroups = getGroupMembers(GroupId);



            return groupData;

        }

        public MyResponse AddPermissionToGroup(AddPermissionToGroup addPermissionToGroup)
        {
            MyResponse response = new MyResponse();
            StringBuilder sb = new StringBuilder();
            foreach (var a in addPermissionToGroup.Permissions)
            {
                sb.Append("INSERT INTO  \"UserGroupsPermission\" (\"group_id\",\"permission\") VALUES ('" + addPermissionToGroup.groupId + "','" + a + "'); ");

            }
            string query = sb.ToString();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            bool myReq1 = myDbconnection.UpdateDelInsert(query, OrganizationId);

            cnn.Close();

            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            else
            {
                response.Httpcode = 200;
                response.Message = "Permission added to group";
            }



            return response;
        }






        public MyResponse AddUserstoGroup(AddUserToGroup addUserToGroup)
        {

            MyResponse response = new MyResponse();
            StringBuilder sb = new StringBuilder();
            foreach (var a in addUserToGroup.users)
            {
                sb.Append("INSERT INTO  \"UserGroups\" (\"group_id\",\"user_id\") VALUES ('" + addUserToGroup.groupId + "','" + a + "'); ");

            }
            string query = sb.ToString();
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            bool myReq1 = myDbconnection.UpdateDelInsert(query, OrganizationId);

            cnn.Close();

            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            else
            {
                response.Httpcode = 200;
                response.Message = "Users added to group";
            }



            return response;



        }

        public MyResponse RemoveUserFromGroup(UserGroups groupPermmissions)
        {


            MyResponse response = new MyResponse();

            string query = "DELETE FROM  \"UserGroups\" WHERE \"group_id\" ='" + groupPermmissions.GroupId + "'  AND \"user_id\" = '" + groupPermmissions.UserId + "'  ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            bool myReq1 = myDbconnection.UpdateDelInsert(query, OrganizationId);

            cnn.Close();

            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            else
            {
                response.Httpcode = 200;
                response.Message = "User has been removed from group";
            }


            return response;
        }

        public MyResponse RemovePermissionFromGroup(GroupPermmissions groupPermmissions)
        {


            MyResponse response = new MyResponse();
          
            string query = "DELETE FROM  \"UserGroupsPermission\" WHERE \"group_id\" ='"+groupPermmissions.GroupId+ "'  AND \"permission\" = '"+groupPermmissions.Permission+"'  ";
            NpgsqlConnection cnn = new NpgsqlConnection(new dbconnection().CheckConn(OrganizationId));
            cnn.Open();
            bool myReq1 = myDbconnection.UpdateDelInsert(query, OrganizationId);

            cnn.Close();

            if (myReq1 == false)
            {
                response.Httpcode = 400;
                response.Message = "An occured while trying to save details.";
            }
            else
            {
                response.Httpcode = 200;
                response.Message = "Permission has been removed from group";
            }


            return response;
        }
    }

}

