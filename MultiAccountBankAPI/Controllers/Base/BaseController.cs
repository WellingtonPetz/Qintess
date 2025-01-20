using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MultiAccountBankAPI.Controllers
{
    /// <summary>
    /// Controlador base para funcionalidades comuns em toda a API, incluindo autenticação JWT.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController : ControllerBase
    {
        private readonly IConfiguration _config;
        /// <summary>
        /// Construtor do BaseController.
        /// </summary>
        /// <param name="config">Configuração da aplicação, incluindo parâmetros JWT.</param>
        public BaseController(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtém o ID do usuário autenticado a partir do token JWT.
        /// </summary>
        /// <returns>
        /// O ID do usuário autenticado.<br/>
        /// Retorna `null` se o token for inválido ou ausente.
        /// </returns>
        /// <remarks>
        /// **Exemplo de token JWT recebido via cabeçalho Authorization:**
        /// 
        ///     Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
        /// 
        /// **Fluxo de autenticação:**
        /// 1. O usuário faz login e recebe um token JWT.
        /// 2. Esse token é enviado em todas as requisições protegidas.
        /// 3. O `BaseController` extrai e valida o token para obter o ID do usuário.
        /// </remarks>
        protected string GetUserIdFromToken()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var principal = ValidateToken(token);
            return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// Valida o token JWT e retorna os claims do usuário autenticado.
        /// </summary>
        /// <param name="token">Token JWT recebido via cabeçalho Authorization.</param>
        /// <returns>
        /// Retorna um objeto `ClaimsPrincipal` contendo as informações do usuário autenticado.<br/>
        /// Retorna `null` se o token for inválido.
        /// </returns>
        /// <remarks>
        /// **Parâmetros de validação do token:**
        /// - Valida emissor (`Issuer`)
        /// - Valida audiência (`Audience`)
        /// - Valida tempo de expiração (`Lifetime`)
        /// - Valida chave de assinatura (`SigningKey`)
        /// </remarks>
        protected ClaimsPrincipal ValidateToken(string token)
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
