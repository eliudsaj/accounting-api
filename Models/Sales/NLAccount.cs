using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Models.Sales
{
    public class NLAccount
    {
        [Key]
        public string NLAccCode { get; set; }
        public string NLAccName { get; set; }
        public string GroupCode { get; set; }
        public string CurCode { get; set; }
    }
    public class BankReconcilationReport
    {
        public DateTime pyDate { get; set; }
        public string CurrentCustomer { get; set; }
        public decimal Amount { get; set; }
        public decimal Dr { get; set; }
        public decimal Cr { get; set; }
        public string NlAccCode { get; set; }
        public string AccountName { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public decimal BankBalance { get; set; }
        public DateTime StatementDate { get; set; } = DateTime.Now;
        public decimal BalancebroughtForward { get; set; }
        public string BankDescriptions { get; set; }
        public decimal AccountBalance { get; set; }
        public int JournalId { get; set; }
        public string PeriodFrom { get; set; }
        public string PeriodTo { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public string pyChequeNumber { get; set; }
        public string Period { get; set; }
        public string CurrencyCode { get; set; }
    }
    public class CashBookHeader
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public decimal bankBalance { get; set; }
        public decimal UnclearedBalance { get; set; }
        public decimal abBalance { get; set; }
        public decimal CashBookBalance { get; set; }
        public decimal cdBalance { get; set; }
        public string PeriodFrom { get; set; }
        public string PeriodTo { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<CashBookDetails> CashBookDetails { get; set; }
    }
    public class CashBookDetails
    {
        public int Id { get; set; }
        public int JournalId { get; set; }
        public int CashBookId { get; set; }
        public bool Status { get; set; } = true;
    }
    public class PurchaseBankReconcilationReport
    {
        public DateTime pyDate { get; set; }
        public string CustName { get; set; }
        public decimal Dr { get; set; }
        public decimal Cr { get; set; }
        public string NlAccCode { get; set; }
    }
    public class BankHistoryRecord
    {
        public DateTime created_on { get; set; }
        public decimal bank_balance { get; set; }
        public decimal uncleared_balance { get; set; }
        public decimal ab_balance { get; set; }
        public decimal cash_book_balance { get; set; }
        public decimal cd_balance { get; set; }
        public decimal dr { get; set; }
        public decimal cr { get; set; }
        public decimal amount { get; set; }
        public int journal_id { get; set; }
        public bool status { get; set; }
        public string account_holder { get; set; }
        public DateTime date_from { get; set; }
        public DateTime date_to { get; set; }
        public DateTime pay_date { get; set; }
        public string period_from { get; set; }
        public string period_to { get; set; }
        public string account_name { get; set; }
        public string account_code { get; set; }
        public string pyChequeNumber { get; set; }
    }
}
