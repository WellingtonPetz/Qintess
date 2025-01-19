namespace MultiAccountBankAPI.Models
{
    public class TransactionModel
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } // Deposit or Withdrawal
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }

}
