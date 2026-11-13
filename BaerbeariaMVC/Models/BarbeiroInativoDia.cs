namespace BaerbeariaMVC.Models
{
    public class BarbeiroInativoDia
    {
        public int Id { get; set; }

        public int BarbeiroId { get; set; }
        public Barbeiro? Barbeiro { get; set; }

        
        public DateTime Data { get; set; }
    }
}
