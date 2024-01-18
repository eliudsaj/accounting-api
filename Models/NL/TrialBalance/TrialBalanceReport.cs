namespace pyme_finance_api.Models.NL.TrialBalance
{
    public class TrialBalanceReport
    {
        public string NlAccName { get; set; }

        public string CompanyName { get; set; }

        public string GroupType { get; set; }

        public string GroupCode { get; set; }

        public decimal RunStatus { get; set; }

        public decimal Cr { get; set; } 

        public decimal Dr { get; set; }

        public decimal OpeningBalance {get; set; }  
    }

    public class OpeningTrialBalanceData
    {
        public string NlAccName { get; set; }
        public decimal RunStatus { get; set; }

        public string GroupType { get; set; }


    }
}
