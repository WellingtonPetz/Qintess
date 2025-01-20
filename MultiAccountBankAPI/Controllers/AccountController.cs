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


        /// <summary>
        /// Cria uma nova conta bancária.
        /// </summary>
        /// <param name="account">Objeto contendo os dados da conta</param>
        /// <returns>Retorna mensagem de sucesso e a conta criada</returns>
        /// <response code="200">Conta criada com sucesso</response>
        /// <response code="400">Dados inválidos</response>
        /// <response code="401">Usuário não autenticado</response>
        [HttpPost("create")]
        public async Task<IActionResult> CreateAccount([FromBody] BankAccount account)
        {
            var userId = GetUserIdFromToken();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            if (account == null)
                return BadRequest(new { message = "Dados inválidos" });

            account.user_id = userId;
            account.current_balance = 0;
            account.date_created = DateTime.UtcNow;

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Conta criada com sucesso!", account });
        }

        /// <summary>
        /// Obtém todas as contas do usuário autenticado.
        /// </summary>
        /// <returns>Lista de contas</returns>
        /// <response code="200">Retorna as contas do usuário</response>
        /// <response code="401">Usuário não autenticado</response>
        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            var userId = GetUserIdFromToken();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            var accounts = await _context.Accounts
                .Where(a => a.user_id == userId)
                .ToListAsync();

            return Ok(accounts);
        }

        /// <summary>
        /// Exclui uma conta bancária do usuário autenticado.
        /// </summary>
        /// <param name="accountId">O ID da conta a ser excluída.</param>
        /// <returns>
        /// - 200 OK: Conta excluída com sucesso.<br/>
        /// - 400 BadRequest: Conta precisa estar com saldo 0.<br/>
        /// - 401 Unauthorized: Usuário não autenticado.<br/>
        /// - 404 NotFound: Conta não encontrada ou não pertence ao usuário.
        /// </returns>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     DELETE /api/accounts/5
        /// 
        /// </remarks>
        /// <response code="200">Conta excluída com sucesso</response>
        /// <response code="400">A conta precisa estar com saldo 0 para ser excluída</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="404">Conta não encontrada ou não pertence ao usuário</response>
        [HttpDelete("{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAccount(int accountId)
        {
            var userId = GetUserIdFromToken();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            var account = await _context.Accounts
                                .FirstOrDefaultAsync(a => a.id == accountId && a.user_id == userId);

            if (account == null)
                return NotFound("Conta não encontrada ou não pertence ao usuário.");


            if (account.current_balance > 0)
                return BadRequest("A conta precisa estar com saldo 0 para ser excluída.");

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return Ok("Conta excluída com sucesso.");
        }
    }
}
