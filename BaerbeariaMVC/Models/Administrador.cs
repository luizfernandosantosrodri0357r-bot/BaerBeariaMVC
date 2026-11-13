using System.ComponentModel.DataAnnotations;

namespace BaerbeariaMVC.Models
{
    public class Administrador
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Usuario { get; set; } = string.Empty;

        
        [Required]
        public string SenhaHash { get; set; } = string.Empty;
    }
}