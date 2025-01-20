using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MultiAccountBankAPI.DTO;
using MultiAccountBankAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MultiAccountBankAPI.Controllers
{
    /// <summary>
    /// Controlador responsável pela autenticação e gerenciamento de usuários.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;

        /// <summary>
        /// Construtor da AuthController.
        /// </summary>
        /// <param name="userManager">Gerenciador de usuários.</param>
        /// <param name="signInManager">Gerenciador de autenticação.</param>
        /// <param name="config">Configurações do JWT.</param>
        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }


        /// <summary>
        /// Registra um novo usuário na aplicação.
        /// </summary>
        /// <param name="request">Objeto contendo e-mail, nome e senha do usuário.</param>
        /// <returns>
        /// - 200 OK: Usuário registrado com sucesso.<br/>
        /// - 400 BadRequest: Erros de validação ou falha no registro.
        /// </returns>
        /// <remarks>
        /// **Exemplo de requisição:**
        /// 
        ///     POST /api/auth/register
        ///     {
        ///        "email": "usuario@email.com",
        ///        "name": "Nome do Usuário",
        ///        "password": "SenhaSegura123!"
        ///     }
        /// </remarks>
        /// <response code="200">Usuário registrado com sucesso</response>
        /// <response code="400">Erro de validação ou falha no registro</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                Name = request.Name
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Usuário registrado com sucesso!");
        }

        /// <summary>
        /// Realiza login na aplicação e retorna um token JWT.
        /// </summary>
        /// <param name="model">Credenciais do usuário (e-mail e senha).</param>
        /// <returns>
        /// - 200 OK: Retorna o token JWT.<br/>
        /// - 401 Unauthorized: Credenciais inválidas.
        /// </returns>
        /// <remarks>
        /// **Exemplo de requisição:**
        /// 
        ///     POST /api/auth/login
        ///     {
        ///        "email": "usuario@email.com",
        ///        "password": "SenhaSegura123!"
        ///     }
        /// 
        /// **Exemplo de resposta:**
        /// 
        ///     {
        ///        "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6..."
        ///     }
        /// </remarks>
        /// <response code="200">Retorna o token JWT</response>
        /// <response code="401">Credenciais inválidas</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.CheckPasswordAsync(user, model.Password)))
                return Unauthorized();

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        /// <summary>
        /// Gera um token JWT para um usuário autenticado.
        /// </summary>
        /// <param name="user">Usuário autenticado.</param>
        /// <returns>Token JWT válido por 30 minutos.</returns>
        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // User ID
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
