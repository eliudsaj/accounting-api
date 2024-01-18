using pyme_finance_api.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Dashboard
{
    public class DahboardResponse
    {
        public int totalUsers { get; set; }

        public int totalsalescustomers { get; set; }

        public int totalbranches { get; set; }

        public int totalinvoicedue { get; set; }

        public int totalpurchasecustomers { get; set; }

        public int totalinventory { get; set; }

        public int invoicesduecount { get; set; }

        public List<Salesanalysis> salesanalysisdata { get; set; }

        public List<MonthlyPayments> monthlyPayments { get; set; }

        public List<InvoiceData> invoiceDatas { get; set; }

        public List<Purchaseanalysis> purchaseAnalysis { get; set; }

        public FinancialPeriod financialPeriod { get; set; }

        public List<MonthlySales> monthlySales { get; set; }

        public List<MonthlySales> monthlyPurchases { get; set; }
    }


    public class Salesanalysis
    {
        public string name { get; set; }

        public int soldquantity { get; set;  }

        public decimal total { get; set; }
    }


    public class MonthlyPayments
    {
        public string month { get; set; }

        public decimal amount { get; set; }

    }

    public class MonthlySales
    {
        public string period { get; set; }

        public decimal amount { get; set; }

    }

    public class InvoiceData
    {
        public string name { get; set; }

        public decimal amount { get; set; }


        public InvoiceData(string _name, decimal _amount)
        {
            this.name = _name;
            this.amount = _amount;
        }
    }

    public class Purchaseanalysis
    {
        public string name { get; set; }

        public int soldquantity { get; set; }

        public decimal total { get; set; }
    }




}
