using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Authentication
{
    public class Group
    {

        public int Id { get; set; }
        public string Name { get; set; }

        public string Status { get; set; }

        public DateTime CreatedOn { get; set; }

        public int CreatedBy { get; set; }

     



    }

    public class UserGroups
    {
        public int GroupId { get; set; }

        public int UserId { get; set; }
    }


    public class GroupPermmissions
    {
        public int GroupId { get; set; }

        public string Permission { get; set; }
    }

    public class AddGroupRequest
    {
        public string Name { get; set; }

        public List<long> Users { get; set; }

        public string[] Permissions { get; set; }
    }

    public class GroupData
    {
        public string Name { get; set; }

        public List<UserGroups> UserGroups { get; set;  }

        public List<GroupPermmissions> GroupPermmissions { get; set; }
    }


    public class AddPermissionToGroup
    {
        public string[] Permissions { get; set; }

        public int groupId { get; set; }

    }


    public class AddUserToGroup
    {
        public int[] users { get; set; }

        public int groupId { get; set; }

    }
}
