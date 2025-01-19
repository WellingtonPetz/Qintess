using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiAccountBankAPI.Data;
using MultiAccountBankAPI.Models;
using System.Security.Claims;

namespace MultiAccountBankAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Realizar depósito ou saque
        [HttpPost("create")]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionModel transaction)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var account = await _context.Accounts.FindAsync(transaction.AccountId);
            if (account == null || account.UserId != userId) return NotFound("Conta não encontrada.");

            if (transaction.Type == "Withdrawal" && account.CurrentBalance < transaction.Amount)
                return BadRequest("Saldo insuficiente para saque.");

            // Aplicar a transação
            account.CurrentBalance += (transaction.Type == "Deposit" ? transaction.Amount : -transaction.Amount);
            transaction.TransactionDate = DateTime.UtcNow;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(transaction);
        }

        // 🔹 Listar transações de uma conta
        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetTransactions(int accountId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var transactions = await _context.Transactions
                .Where(t => t.AccountId == accountId && _context.Accounts.Any(a => a.Id == accountId && a.UserId == userId))
                .ToListAsync();

            return Ok(transactions);
        }
    }
}
