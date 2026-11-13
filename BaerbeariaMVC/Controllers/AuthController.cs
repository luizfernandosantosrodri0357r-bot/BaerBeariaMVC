using BaerbeariaMVC.Data;
using BaerbeariaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BaerbeariaMVC.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        
        public class LoginDto
        {
            public string Usuario { get; set; } = string.Empty;
            public string Senha { get; set; } = string.Empty;
        }

        
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var admin = await _context.Administradores
                .FirstOrDefaultAsync(a => a.Usuario == dto.Usuario);

            
            if (admin == null)
            {
                return Unauthorized(new { mensagem = "Usuário ou senha inválidos." });
            }

            var hasher = new PasswordHasher<Administrador>();
            var resultado = hasher.VerifyHashedPassword(admin, admin.SenhaHash, dto.Senha);

            if (resultado == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { mensagem = "Usuário ou senha inválidos." });
            }

            
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, admin.Usuario),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Chave"]!));
            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

            
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Emissor"],
                audience: _config["Jwt:Audiencia"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credenciais);

            var tokenTexto = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { token = tokenTexto });
        }
    }
}