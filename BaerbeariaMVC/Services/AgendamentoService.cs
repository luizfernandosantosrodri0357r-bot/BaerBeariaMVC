using BaerbeariaMVC.Data;
using BaerbeariaMVC.Dtos;
using BaerbeariaMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace BaerbeariaMVC.Services
{
    public class AgendamentoService
    {
        private readonly AppDbContext _context;

        
        private const int HoraAbertura = 8;
        private const int HoraFechamento = 18;

        public AgendamentoService(AppDbContext context)
        {
            _context = context;
        }

        
        public async Task<List<DateTime>> ObterHorariosDisponiveis(int barbeiroId, DateTime data)
        {
            var horarios = new List<DateTime>();
            var dia = data.Date;

            
            var barbeiro = await _context.Barbeiros.FindAsync(barbeiroId);
            if (barbeiro == null || !barbeiro.Ativo)
            {
                return horarios;
            }

            
            var inativoNoDia = await _context.BarbeirosInativosDia
                .AnyAsync(b => b.BarbeiroId == barbeiroId && b.Data.Date == dia);
            if (inativoNoDia)
            {
                return horarios;
            }

            
            var ocupados = await _context.Agendamentos
                .Where(a => a.BarbeiroId == barbeiroId
                            && !a.Cancelado
                            && a.DataHora.Date == dia)
                .Select(a => a.DataHora)
                .ToListAsync();

            
            for (int hora = HoraAbertura; hora < HoraFechamento; hora++)
            {
                var horario = dia.AddHours(hora);

                if (horario <= DateTime.Now)
                {
                    continue;
                }

                if (ocupados.Contains(horario))
                {
                    continue;
                }

                horarios.Add(horario);
            }

            return horarios;
        }

        
        public async Task<(Agendamento? agendamento, string? erro)> Criar(AgendamentoCriarDto dto)
        {
            
            var telefone = new string(dto.Telefone.Where(char.IsDigit).ToArray());
            if (telefone.Length < 10 || telefone.Length > 11)
            {
                return (null, "Telefone inválido. Informe o DDD e o número.");
            }
            
            var horarioValido = dto.DataHora.Minute == 0
                                && dto.DataHora.Second == 0
                                && dto.DataHora.Hour >= HoraAbertura
                                && dto.DataHora.Hour < HoraFechamento;
            if (!horarioValido)
            {
                return (null, "Horário inválido. Atendemos de 08h às 18h, em horas cheias.");
            }

            if (dto.DataHora <= DateTime.Now)
            {
                return (null, "Não é possível agendar em um horário que já passou.");
            }

            var servicoExiste = await _context.Servicos.AnyAsync(s => s.Id == dto.ServicoId);
            if (!servicoExiste)
            {
                return (null, "Serviço não encontrado.");
            }

            
            var livres = await ObterHorariosDisponiveis(dto.BarbeiroId, dto.DataHora);
            if (!livres.Contains(dto.DataHora))
            {
                return (null, "Horário indisponível para este barbeiro.");
            }

            var agendamento = new Agendamento
            {
                BarbeiroId = dto.BarbeiroId,
                ServicoId = dto.ServicoId,
                DataHora = dto.DataHora,
                NomeCliente = dto.NomeCliente,
                Telefone = telefone
            };

            _context.Agendamentos.Add(agendamento);
            await _context.SaveChangesAsync();

            return (agendamento, null);
        }
    }
}