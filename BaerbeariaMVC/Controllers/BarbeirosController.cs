using BaerbeariaMVC.Data;
using BaerbeariaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaerbeariaMVC.Controllers
{
    
    [ApiController]
    
    [Route("api/barbeiros")]
    public class BarbeirosController : ControllerBase
    {
        private readonly AppDbContext _context;

        
        public BarbeirosController(AppDbContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var barbeiros = await _context.Barbeiros.ToListAsync();
            return Ok(barbeiros);
        }

        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cadastrar(Barbeiro barbeiro)
        {
            _context.Barbeiros.Add(barbeiro);
            await _context.SaveChangesAsync();
            return Ok(barbeiro);
            
        }
    }
}