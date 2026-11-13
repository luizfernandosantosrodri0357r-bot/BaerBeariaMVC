using System.ComponentModel.DataAnnotations;

namespace BaerbeariaMVC.Models
{
    public class Agendamento
    {
        public int Id { get; set; }

        
        public int BarbeiroId { get; set; }
        public Barbeiro? Barbeiro { get; set; }

        
        public int ServicoId { get; set; }
        public Servico? Servico { get; set; }

        
        public DateTime DataHora { get; set; }

        [Required]
        [StringLength(100)]
        public string NomeCliente { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Telefone { get; set; } = string.Empty;

        
        public bool Cancelado { get; set; } = false;
    }
}