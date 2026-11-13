using BaerbeariaMVC.Data;
using BaerbeariaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaerbeariaMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PainelController : Controller
    {
        private readonly AppDbContext _context;

        public PainelController(AppDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index(DateTime? data)
        {
            var dia = (data ?? DateTime.Today).Date;

            var lista = await _context.Agendamentos
                .Include(a => a.Barbeiro)
                .Include(a => a.Servico)
                .Where(a => a.DataHora.Date == dia)
                .OrderBy(a => a.DataHora)
                .ToListAsync();

            ViewBag.Data = dia.ToString("yyyy-MM-dd");
            return View(lista);
        }

        
        [HttpPost]
        public async Task<IActionResult> Cancelar(int id, string data)
        {
            var agendamento = await _context.Agendamentos.FindAsync(id);

            if (agendamento != null && !agendamento.Cancelado)
            {
                agendamento.Cancelado = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { data });
        }

        
        public async Task<IActionResult> Barbeiros()
        {
            ViewBag.Dias = await _context.BarbeirosInativosDia
                .Include(d => d.Barbeiro)
                .Where(d => d.Data >= DateTime.Today)
                .OrderBy(d => d.Data)
                .ToListAsync();

            var lista = await _context.Barbeiros
                .OrderBy(b => b.Nome)
                .ToListAsync();

            return View(lista);
        }

        
        [HttpPost]
        public async Task<IActionResult> CadastrarBarbeiro(string nome)
        {
            if (!string.IsNullOrWhiteSpace(nome) && nome.Trim().Length <= 100)
            {
                _context.Barbeiros.Add(new Barbeiro { Nome = nome.Trim() });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Barbeiros");
        }

        
        [HttpPost]
        public async Task<IActionResult> AlternarAtivo(int id)
        {
            var barbeiro = await _context.Barbeiros.FindAsync(id);

            if (barbeiro != null)
            {
                barbeiro.Ativo = !barbeiro.Ativo;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Barbeiros");
        }

        
        [HttpPost]
        public async Task<IActionResult> InativarDia(int id, DateTime data)
        {
            var dia = data.Date;

            var barbeiro = await _context.Barbeiros.FindAsync(id);
            if (barbeiro == null)
            {
                return RedirectToAction("Barbeiros");
            }

            var jaMarcado = await _context.BarbeirosInativosDia
                .AnyAsync(b => b.BarbeiroId == id && b.Data == dia);

            if (!jaMarcado)
            {
                _context.BarbeirosInativosDia.Add(new BarbeiroInativoDia
                {
                    BarbeiroId = id,
                    Data = dia
                });
                await _context.SaveChangesAsync();

                var marcados = await _context.Agendamentos
                    .CountAsync(a => a.BarbeiroId == id && !a.Cancelado && a.DataHora.Date == dia);

                if (marcados > 0)
                {
                    TempData["Aviso"] = "Atenção: " + barbeiro.Nome + " já tem " + marcados +
                        " agendamento(s) em " + dia.ToString("dd/MM/yyyy") +
                        ". Cancele-os manualmente no painel de agendamentos.";
                }
            }

            return RedirectToAction("Barbeiros");
        }

        
        [HttpPost]
        public async Task<IActionResult> ReativarDia(int id)
        {
            var registro = await _context.BarbeirosInativosDia.FindAsync(id);

            if (registro != null)
            {
                _context.BarbeirosInativosDia.Remove(registro);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Barbeiros");
        } 

        
        public async Task<IActionResult> Servicos()
        {
            var lista = await _context.Servicos.OrderBy(s => s.Id).ToListAsync();
            return View(lista);
        }

        
        [HttpPost]
        public async Task<IActionResult> AlterarPreco(int id, string preco)
        {
            var servico = await _context.Servicos.FindAsync(id);
            if (servico == null)
            {
                return RedirectToAction("Servicos");
            }

            
            var texto = (preco ?? "").Trim().Replace(',', '.');
            var valido = decimal.TryParse(texto,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out var valor);

            if (!valido || valor < 0 || valor > 10000)
            {
                TempData["Aviso"] = "Valor inválido. Digite um número, por exemplo 75,00.";
                return RedirectToAction("Servicos");
            }

            servico.Preco = Math.Round(valor, 2);
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Valor de " + servico.Nome + " atualizado.";
            return RedirectToAction("Servicos");
        }

        
        [HttpPost]
        public async Task<IActionResult> ExcluirBarbeiro(int id)
        {
            var barbeiro = await _context.Barbeiros.FindAsync(id);
            if (barbeiro == null)
            {
                return RedirectToAction("Barbeiros");
            }

            var temAgendamentos = await _context.Agendamentos.AnyAsync(a => a.BarbeiroId == id);
            if (temAgendamentos)
            {
                TempData["Aviso"] = barbeiro.Nome + " tem agendamentos no histórico e não pode ser excluído. " +
                    "Use o botão Desativar para ele deixar de aparecer para os clientes.";
                return RedirectToAction("Barbeiros");
            }

            
            var dias = _context.BarbeirosInativosDia.Where(d => d.BarbeiroId == id);
            _context.BarbeirosInativosDia.RemoveRange(dias);
            _context.Barbeiros.Remove(barbeiro);
            await _context.SaveChangesAsync();

            TempData["Ok"] = barbeiro.Nome + " foi excluído.";
            return RedirectToAction("Barbeiros");
        }
    }
}