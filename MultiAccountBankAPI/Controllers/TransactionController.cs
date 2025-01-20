using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiAccountBankAPI.Data;
using MultiAccountBankAPI.Models;
using System.Security.Claims;

namespace MultiAccountBankAPI.Controllers
{
    /// <summary>
    /// Controlador responsável por operações de transações bancárias, como depósitos e saques.
    /// </summary>
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : BaseController
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor do TransactionController.
        /// </summary>
        /// <param name="context">Contexto do banco de dados.</param>
        /// <param name="config">Configurações da aplicação.</param>
        public TransactionController(ApplicationDbContext context, IConfiguration config) : base(config)
        {
            _context = context;
        }
        /// <summary>
        /// Realiza um depósito ou saque em uma conta do usuário autenticado.
        /// </summary>
        /// <param name="transaction">Objeto contendo os detalhes da transação.</param>
        /// <returns>
        /// - 200 OK: Transação realizada com sucesso.<br/>
        /// - 401 Unauthorized: Usuário não autenticado.<br/>
        /// - 404 Not Found: Conta não encontrada.<br/>
        /// - 400 Bad Request: Saldo insuficiente para saque.
        /// </returns>
        /// <remarks>
        /// **Exemplo de requisição para depósito:**
        /// 
        ///     POST /api/transactions/create
        ///     {
        ///         "accountId": 1,
        ///         "amount": 500.00,
        ///         "type": "Deposit"
        ///     }
        ///
        /// **Exemplo de requisição para saque:**
        /// 
        ///     POST /api/transactions/create
        ///     {
        ///         "accountId": 1,
        ///         "amount": 200.00,
        ///         "type": "Withdrawal"
        ///     }
        ///
        /// **Exemplo de resposta (sucesso):**
        /// 
        ///     {
        ///         "accountId": 1,
        ///         "amount": 500.00,
        ///         "type": "Deposit",
        ///         "transactionDate": "2024-01-20T12:34:56Z"
        ///     }
        /// </remarks>
        /// <response code="200">Transação realizada com sucesso</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="404">Conta não encontrada</response>
        /// <response code="400">Saldo insuficiente para saque</response>
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionModel transaction)
        {
            var userId = GetUserIdFromToken();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

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

        /// <summary>
        /// Obtém a lista de transações de uma conta específica do usuário autenticado.
        /// </summary>
        /// <param name="accountId">ID da conta.</param>
        /// <returns>
        /// - 200 OK: Retorna a lista de transações.<br/>
        /// - 401 Unauthorized: Usuário não autenticado.<br/>
        /// - 404 Not Found: Nenhuma transação encontrada para essa conta.
        /// </returns>
        /// <remarks>
        /// **Exemplo de requisição:**
        /// 
        ///     GET /api/transactions/1
        /// 
        /// **Exemplo de resposta:**
        /// 
        ///     [
        ///         { "transactionId": 1, "accountId": 1, "amount": 500.00, "type": "Deposit", "transactionDate": "2024-01-20T12:34:56Z" },
        ///         { "transactionId": 2, "accountId": 1, "amount": 200.00, "type": "Withdrawal", "transactionDate": "2024-01-21T10:15:30Z" }
        ///     ]
        /// </remarks>
        /// <response code="200">Retorna a lista de transações</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="404">Nenhuma transação encontrada</response>
        [HttpGet("{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransactions(int accountId)
        {
            var userId = GetUserIdFromToken();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            var transactions = await _context.Transactions
                .Where(t => t.AccountId == accountId && _context.Accounts.Any(a => a.Id == accountId && a.UserId == userId))
                .ToListAsync();

            return Ok(transactions);
        }
    }
}
