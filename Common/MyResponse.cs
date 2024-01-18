using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Common
{
    public class MyResponse
    {
        public int Httpcode { get; set; }
        public int Id { get; set; }
        public string Message { get; set; }
    }
}
