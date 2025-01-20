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
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AccountController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [AllowAnonymous]
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
        [AllowAnonymous]
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

        [AllowAnonymous]
        [HttpDelete("{accountId}")]
        public async Task<IActionResult> DeleteAccount(int accountId)
        {
            var userId = GetUserIdFromToken();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null || account.UserId != userId) return NotFound();

            if (account.CurrentBalance > 0)
                return BadRequest("A conta precisa estar com saldo 0 para ser excluída.");

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return Ok("Conta excluída com sucesso.");
        }

        private string GetUserIdFromToken()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var principal = ValidateToken(token);
            var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return userId;
        }

        private ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]))
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
