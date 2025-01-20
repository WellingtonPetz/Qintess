namespace MultiAccountBankAPI.Models
{
    public class BankAccount
    {
        public int id { get; set; }
        public string user_id { get; set; }
        public string account_name { get; set; }
        public decimal current_balance { get; set; } = 0;
        public DateTime date_created { get; set; } = DateTime.UtcNow;
    }

}
