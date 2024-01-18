using System.Collections.Generic;

namespace pyme_finance_api.Models.NL.BalanceSheet
{
    public class BalanceSheet
    {

        public List<BalanceSheetDetails> CurrentAssets { get; set; }

        public List<BalanceSheetDetails> CurrentLiabilities { get; set; }

        public List<BalanceSheetDetails> NoncurrentAssets  { get; set; }

        public List<BalanceSheetDetails> NoncurrentLiabilities { get; set; }


        public List<BalanceSheetDetails> Equity { get; set; }

    }


    public class BalanceSheetDetails
    {
        public string AccountName { get; set; }

        public decimal Ammount { get; set; }

    }
}
