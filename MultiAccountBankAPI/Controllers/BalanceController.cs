using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiAccountBankAPI.Data;
using System.Security.Claims;

namespace MultiAccountBankAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/balance")]
    public class BalanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BalanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Consultar saldo de uma conta específica
        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetBalance(int accountId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

            if (account == null) return NotFound("Conta não encontrada.");

            return Ok(new { Balance = account.CurrentBalance });
        }

        // 🔹 Resumo de todas as contas do usuário
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => new { a.AccountName, a.CurrentBalance })
                .ToListAsync();

            return Ok(accounts);
        }
    }
}
