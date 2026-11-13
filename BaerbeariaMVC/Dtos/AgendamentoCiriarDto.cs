using System.ComponentModel.DataAnnotations;

namespace BaerbeariaMVC.Dtos
{
    
    public class AgendamentoCriarDto
    {
        public int BarbeiroId { get; set; }

        public int ServicoId { get; set; }

        
        public DateTime DataHora { get; set; }

        [Required]
        [StringLength(100)]
        public string NomeCliente { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Telefone { get; set; } = string.Empty;
    }
}