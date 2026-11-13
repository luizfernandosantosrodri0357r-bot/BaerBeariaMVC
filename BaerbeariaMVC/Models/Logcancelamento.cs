using System.ComponentModel.DataAnnotations;

namespace BaerbeariaMVC.Models
{
    
    public class LogCancelamento
    {
        public int Id { get; set; }

        public int AgendamentoId { get; set; }

        [Required]
        [StringLength(100)]
        public string NomeCliente { get; set; } = string.Empty;

        public DateTime DataHoraAgendamento { get; set; }

        public DateTime DataCancelamento { get; set; }
    }
}