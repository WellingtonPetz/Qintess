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
    [Route("api/accounts")]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Criar uma nova conta
        [HttpPost("create")]
        public async Task<IActionResult> CreateAccount([FromBody] BankAccount account)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            account.UserId = userId;
            account.CurrentBalance = 0; // Inicializa com saldo 0
            account.DateCreated = DateTime.UtcNow;

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(account);
        }

        // 🔹 Listar todas as contas do usuário autenticado
        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return Ok(accounts);
        }

        // 🔹 Deletar uma conta (se saldo for 0)
        [HttpDelete("{accountId}")]
        public async Task<IActionResult> DeleteAccount(int accountId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null || account.UserId != userId) return NotFound();

            if (account.CurrentBalance > 0)
                return BadRequest("A conta precisa estar com saldo 0 para ser excluída.");

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return Ok("Conta excluída com sucesso.");
        }
    }
}
