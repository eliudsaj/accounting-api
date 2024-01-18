namespace pyme_finance_api.Models.Purchases.PurchaseReport
{
    public class AccountsPayableAgeingReport
    {

            public string customer { get; set; }

            public decimal Over90Days { get; set; }

            public decimal sixtoneto90days { get; set; }

            public decimal thirtyoneto60days { get; set; }

            public decimal oneto30days { get; set; }

            public decimal Current { get; set; }

       

    }
}
