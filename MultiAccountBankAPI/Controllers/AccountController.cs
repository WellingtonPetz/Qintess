using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultiAccountBankAPI.Data;
using MultiAccountBankAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MultiAccountBankAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context, IConfiguration config) : base(config)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAccount([FromBody] BankAccount account)
        {
            var userId = GetUserIdFromToken();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            if (account == null)
                return BadRequest(new { message = "Dados inválidos" });

            account.UserId = userId;
            account.CurrentBalance = 0;
            account.DateCreated = DateTime.UtcNow;

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Conta criada com sucesso!", account });
        }
        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            var userId = GetUserIdFromToken();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return Ok(accounts);
        }

        [HttpDelete("{accountId}")]
        public async Task<IActionResult> DeleteAccount(int accountId)
        {
            var userId = GetUserIdFromToken();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            var account = await _context.Accounts
                                .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

            if (account == null)
                return NotFound("Conta não encontrada ou não pertence ao usuário.");


            if (account.CurrentBalance > 0)
                return BadRequest("A conta precisa estar com saldo 0 para ser excluída.");

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return Ok("Conta excluída com sucesso.");
        }
    }
}
