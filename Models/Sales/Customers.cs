using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace pyme_finance_api.Models.Sales
{
    public class Customers
    {
        //customer types
        [Key]
        public int SLCTypeID { get; set; }
        public string TypeName { get; set; }
    }
}
