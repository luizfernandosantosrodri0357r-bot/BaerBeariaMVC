using System.ComponentModel.DataAnnotations;

namespace BaerbeariaMVC.Models
{
    public class Barbeiro
    {
        public int Id { get; set; }

        
        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        
        public bool Ativo { get; set; } = true;
    }
}