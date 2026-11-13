using BaerbeariaMVC.Data;
using BaerbeariaMVC.Dtos;
using BaerbeariaMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaerbeariaMVC.Controllers
{
    
    public class AgendarController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AgendamentoService _service;

        
        private const int DiasVisiveis = 14;

        public AgendarController(AppDbContext context, AgendamentoService service)
        {
            _context = context;
            _service = service;
        }

        
        public async Task<IActionResult> Index()
        {
            var barbeiros = await _context.Barbeiros
                .Where(b => b.Ativo)
                .OrderBy(b => b.Nome)
                .ToListAsync();

            return View(barbeiros);
        }

        
        public async Task<IActionResult> Profissional(int id)
        {
            var barbeiro = await _context.Barbeiros.FindAsync(id);

            if (barbeiro == null || !barbeiro.Ativo)
            {
                return RedirectToAction("Index");
            }

        
            var agenda = new SortedDictionary<DateTime, List<DateTime>>();

            for (int i = 0; i < DiasVisiveis; i++)
            {
                var dia = DateTime.Today.AddDays(i);
                agenda[dia] = await _service.ObterHorariosDisponiveis(id, dia);
            }

            ViewBag.Barbeiro = barbeiro;
            ViewBag.Servicos = await _context.Servicos.ToListAsync();

            return View(agenda);
        }

        
        [HttpPost]
        public async Task<IActionResult> Confirmar(AgendamentoCriarDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Erro = "Preencha o nome e o telefone.";
                return View("Erro");
            }

            var (agendamento, erro) = await _service.Criar(dto);

            if (erro != null)
            {
                ViewBag.Erro = erro;
                return View("Erro");
            }

            var completo = await _context.Agendamentos
                .Include(a => a.Barbeiro)
                .Include(a => a.Servico)
                .FirstAsync(a => a.Id == agendamento!.Id);

            return View("Sucesso", completo);
        }
    }
}