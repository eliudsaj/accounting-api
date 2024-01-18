using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Sales
{
    public class Discounts
    {
        [Key]
        public int DId { get; set; }
        public string DRef { get; set; }
        public float DPerc { get; set; }
        public DateTime DSetDate { get; set; }
        public DateTime DEndDate { get; set; }
    }
}
