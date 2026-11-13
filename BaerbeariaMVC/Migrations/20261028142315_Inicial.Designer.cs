
using System;
using BaerbeariaMVC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BaerbeariaMVC.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261028142315_Inicial")]
    partial class Inicial
    {
        
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BaerbeariaMVC.Models.Administrador", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("SenhaHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Usuario")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Administradores");
                });

            modelBuilder.Entity("BaerbeariaMVC.Models.Agendamento", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BarbeiroId")
                        .HasColumnType("int");

                    b.Property<bool>("Cancelado")
                        .HasColumnType("bit");

                    b.Property<DateTime>("DataHora")
                        .HasColumnType("datetime2");

                    b.Property<string>("NomeCliente")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("ServicoId")
                        .HasColumnType("int");

                    b.Property<string>("Telefone")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("BarbeiroId");

                    b.HasIndex("ServicoId");

                    b.ToTable("Agendamentos");
                });

            modelBuilder.Entity("BaerbeariaMVC.Models.Barbeiro", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Ativo")
                        .HasColumnType("bit");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Barbeiros");
                });

            modelBuilder.Entity("BaerbeariaMVC.Models.BarbeiroInativoDia", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BarbeiroId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Data")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("BarbeiroId");

                    b.ToTable("BarbeirosInativosDia");
                });

            modelBuilder.Entity("BaerbeariaMVC.Models.Servico", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<decimal>("Preco")
                        .HasColumnType("decimal(10,2)");

                    b.HasKey("Id");

                    b.ToTable("Servicos");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Nome = "Corte",
                            Preco = 75m
                        },
                        new
                        {
                            Id = 2,
                            Nome = "Barba",
                            Preco = 70m
                        },
                        new
                        {
                            Id = 3,
                            Nome = "Corte + Barba",
                            Preco = 130m
                        },
                        new
                        {
                            Id = 4,
                            Nome = "Corte + Pigmentação",
                            Preco = 110m
                        },
                        new
                        {
                            Id = 5,
                            Nome = "Corte + Barba + Sobrancelha",
                            Preco = 160m
                        });
                });

            modelBuilder.Entity("BaerbeariaMVC.Models.Agendamento", b =>
                {
                    b.HasOne("BaerbeariaMVC.Models.Barbeiro", "Barbeiro")
                        .WithMany()
                        .HasForeignKey("BarbeiroId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("BaerbeariaMVC.Models.Servico", "Servico")
                        .WithMany()
                        .HasForeignKey("ServicoId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Barbeiro");

                    b.Navigation("Servico");
                });

            modelBuilder.Entity("BaerbeariaMVC.Models.BarbeiroInativoDia", b =>
                {
                    b.HasOne("BaerbeariaMVC.Models.Barbeiro", "Barbeiro")
                        .WithMany()
                        .HasForeignKey("BarbeiroId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Barbeiro");
                });
#pragma warning restore 612, 618
        }
    }
}
