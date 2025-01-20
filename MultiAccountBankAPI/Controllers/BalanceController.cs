using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiAccountBankAPI.Data;
using System.Security.Claims;

namespace MultiAccountBankAPI.Controllers
{
    /// <summary>
    /// Controlador responsável pela consulta de saldo e resumo das contas do usuário.
    /// </summary>
    [ApiController]
    [Route("api/balance")]
    public class BalanceController : BaseController
    {
        private readonly ApplicationDbContext _context;
        /// <summary>
        /// Construtor do BalanceController.
        /// </summary>
        /// <param name="context">Contexto do banco de dados.</param>
        /// <param name="config">Configurações da aplicação.</param>
        public BalanceController(ApplicationDbContext context, IConfiguration config) : base(config)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém o saldo de uma conta específica do usuário autenticado.
        /// </summary>
        /// <param name="accountId">ID da conta a ser consultada.</param>
        /// <returns>
        /// - 200 OK: Retorna o saldo da conta.<br/>
        /// - 401 Unauthorized: Usuário não autenticado.<br/>
        /// - 404 Not Found: Conta não encontrada.
        /// </returns>
        /// <remarks>
        /// **Exemplo de requisição:**
        /// 
        ///     GET /api/balance/1
        /// 
        /// **Exemplo de resposta:**
        /// 
        ///     {
        ///        "balance": 2500.75
        ///     }
        /// </remarks>
        /// <response code="200">Retorna o saldo da conta</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="404">Conta não encontrada</response>
        [HttpGet("{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBalance(int accountId)
        {
            var userId = GetUserIdFromToken();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

            if (account == null) return NotFound("Conta não encontrada.");

            return Ok(new { Balance = account.CurrentBalance });
        }

        /// <summary>
        /// Obtém um resumo de todas as contas do usuário autenticado.
        /// </summary>
        /// <returns>
        /// - 200 OK: Retorna a lista de contas e seus saldos.<br/>
        /// - 401 Unauthorized: Usuário não autenticado.
        /// </returns>
        /// <remarks>
        /// **Exemplo de requisição:**
        /// 
        ///     GET /api/balance/summary
        /// 
        /// **Exemplo de resposta:**
        /// 
        ///     [
        ///        { "accountName": "Conta Corrente", "currentBalance": 1500.50 },
        ///        { "accountName": "Poupança", "currentBalance": 5000.00 }
        ///     ]
        /// </remarks>
        /// <response code="200">Retorna a lista de contas e seus saldos</response>
        /// <response code="401">Usuário não autenticado</response>
        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSummary()
        {
            var userId = GetUserIdFromToken();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => new { a.AccountName, a.CurrentBalance })
                .ToListAsync();

            return Ok(accounts);
        }
    }
}
