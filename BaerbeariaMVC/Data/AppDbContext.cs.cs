using BaerbeariaMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace BaerbeariaMVC.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Barbeiro> Barbeiros { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Agendamento> Agendamentos { get; set; }
        public DbSet<BarbeiroInativoDia> BarbeirosInativosDia { get; set; }
        public DbSet<LogCancelamento> LogsCancelamento { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Barbeiro)
                .WithMany()
                .HasForeignKey(a => a.BarbeiroId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Servico)
                .WithMany()
                .HasForeignKey(a => a.ServicoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BarbeiroInativoDia>()
                .HasOne(b => b.Barbeiro)
                .WithMany()
                .HasForeignKey(b => b.BarbeiroId)
                .OnDelete(DeleteBehavior.Restrict);

            
            modelBuilder.Entity<Agendamento>()
                .HasIndex(a => new { a.BarbeiroId, a.DataHora })
                .IsUnique()
                .HasFilter("[Cancelado] = 0");

            
            modelBuilder.Entity<Agendamento>().ToTable(t =>
            {
                t.HasTrigger("trg_Agendamentos_Cancelamento");
                t.HasTrigger("trg_Agendamentos_NaoApagar");
            });

            modelBuilder.Entity<Servico>().HasData(
                new Servico { Id = 1, Nome = "Corte", Preco = 75m },
                new Servico { Id = 2, Nome = "Barba", Preco = 70m },
                new Servico { Id = 3, Nome = "Corte + Barba", Preco = 130m },
                new Servico { Id = 4, Nome = "Corte + Pigmentação", Preco = 110m },
                new Servico { Id = 5, Nome = "Corte + Barba + Sobrancelha", Preco = 160m }
            );
        }
    }
}