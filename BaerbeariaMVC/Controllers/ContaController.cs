using BaerbeariaMVC.Data;
using BaerbeariaMVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BaerbeariaMVC.Controllers
{
    public class ContaController : Controller
    {
        private readonly AppDbContext _context;

        public ContaController(AppDbContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> Login(string usuario, string senha)
        {
            var admin = await _context.Administradores
                .FirstOrDefaultAsync(a => a.Usuario == usuario);

            if (admin != null)
            {
                var hasher = new PasswordHasher<Administrador>();
                var resultado = hasher.VerifyHashedPassword(admin, admin.SenhaHash, senha ?? "");

                if (resultado != PasswordVerificationResult.Failed)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, admin.Usuario),
                        new Claim(ClaimTypes.Role, "Admin")
                    };

                    var identidade = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identidade));

                    return RedirectToAction("Index", "Painel");
                }
            }

            ViewBag.Erro = "Usuário ou senha inválidos.";
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> Sair()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}