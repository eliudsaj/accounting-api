using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.UserProfile
{
    public class usersignature
    {
        public int sign_id { get; set; }
        public DateTime sign_date { get; set; }
        public int sign_user { get; set; }
        public string sign_data { get; set; }
        public string sign_name { get; set; }
    }
}
