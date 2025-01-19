using Microsoft.EntityFrameworkCore;
using MultiAccountBankAPI.Models;
using System.Collections.Generic;

namespace MultiAccountBankAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<BankAccount> Accounts { get; set; }
        public DbSet<TransactionModel> Transactions { get; set; }
    }

}
