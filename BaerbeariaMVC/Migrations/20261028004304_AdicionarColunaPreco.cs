using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaerbeariaMVC.Migrations
{
    
    public partial class AdicionarColunaPreco : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Preco",
                table: "Servicos",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 1,
                column: "Preco",
                value: 75m);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 2,
                column: "Preco",
                value: 70m);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 3,
                column: "Preco",
                value: 130m);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 4,
                column: "Preco",
                value: 110m);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 5,
                column: "Preco",
                value: 160m);
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Preco",
                table: "Servicos");
        }
    }
}
