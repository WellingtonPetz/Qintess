namespace MultiAccountBankAPI.Models
{
    public class BankAccount
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string AccountName { get; set; }
        public decimal CurrentBalance { get; set; } = 0;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }

}
