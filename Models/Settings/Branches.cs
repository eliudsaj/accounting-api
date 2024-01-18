using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using pyme_finance_api.Models.Authentication;

namespace pyme_finance_api.Models.Settings
{
    public class Branches
    {
        [Key]
        public int BrId { get; set; }
        public string BrName { get; set; }
        public string BrLocation { get; set; }
        public string BrCode { get; set; }
        public int ContactStaff { get; set; }

        public bool BrActive { get; set; }

        [JsonIgnore]
        public virtual string UFirstName { get; set; }
        [JsonIgnore]
        public virtual string ULastName { get; set; }
        //public Users Users { get; set; }
    }
}
