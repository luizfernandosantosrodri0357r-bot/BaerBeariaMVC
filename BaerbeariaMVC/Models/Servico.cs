using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaerbeariaMVC.Models
{
    public class Servico
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Preco { get; set; }
    }
}