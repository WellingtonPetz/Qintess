namespace MultiAccountBankAPI.Models
{
    public class TransactionModel
    {
        public int id { get; set; }
        public int account_id { get; set; }
        public decimal amount { get; set; }
        public string type { get; set; } // Deposit or Withdrawal
        public DateTime transaction_date { get; set; } = DateTime.UtcNow;
    }

}
