using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaerbeariaMVC.Migrations
{
    
    public partial class BancoAvancado : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Agendamentos_BarbeiroId",
                table: "Agendamentos");

            migrationBuilder.CreateTable(
                name: "LogsCancelamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgendamentoId = table.Column<int>(type: "int", nullable: false),
                    NomeCliente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataHoraAgendamento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataCancelamento = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsCancelamento", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_BarbeiroId_DataHora",
                table: "Agendamentos",
                columns: new[] { "BarbeiroId", "DataHora" },
                unique: true,
                filter: "[Cancelado] = 0");

            

            
            migrationBuilder.Sql(@"
CREATE TRIGGER trg_Agendamentos_Cancelamento
ON Agendamentos
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO LogsCancelamento (AgendamentoId, NomeCliente, DataHoraAgendamento, DataCancelamento)
    SELECT i.Id, i.NomeCliente, i.DataHora, SYSDATETIME()
    FROM inserted i
    INNER JOIN deleted d ON d.Id = i.Id
    WHERE i.Cancelado = 1 AND d.Cancelado = 0;
END");

            
            migrationBuilder.Sql(@"
CREATE TRIGGER trg_Agendamentos_NaoApagar
ON Agendamentos
INSTEAD OF DELETE
AS
BEGIN
    RAISERROR(N'Agendamentos não podem ser apagados. Use o cancelamento.', 16, 1);
END");

            

            
            migrationBuilder.Sql(@"
CREATE PROCEDURE usp_AgendamentosDoDia
    @Data DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.Id, a.DataHora, a.NomeCliente, a.Telefone,
           b.Nome AS Barbeiro, s.Nome AS Servico, a.Cancelado
    FROM Agendamentos a
    INNER JOIN Barbeiros b ON b.Id = a.BarbeiroId
    INNER JOIN Servicos  s ON s.Id = a.ServicoId
    WHERE a.DataHora >= @Data
      AND a.DataHora < DATEADD(DAY, 1, @Data)
    ORDER BY a.DataHora;
END");

            
            migrationBuilder.Sql(@"
CREATE PROCEDURE usp_HorariosLivres
    @BarbeiroId INT,
    @Data       DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Barbeiros WHERE Id = @BarbeiroId AND Ativo = 1)
    BEGIN
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM BarbeirosInativosDia
               WHERE BarbeiroId = @BarbeiroId AND CAST(Data AS DATE) = @Data)
    BEGIN
        RETURN;
    END

    SELECT DATEADD(HOUR, h.Hora, CAST(@Data AS DATETIME2)) AS Horario
    FROM (VALUES (8), (9), (10), (11), (12), (13), (14), (15), (16), (17)) AS h(Hora)
    WHERE DATEADD(HOUR, h.Hora, CAST(@Data AS DATETIME2)) > SYSDATETIME()
      AND NOT EXISTS (
            SELECT 1
            FROM Agendamentos a
            WHERE a.BarbeiroId = @BarbeiroId
              AND a.Cancelado = 0
              AND a.DataHora = DATEADD(HOUR, h.Hora, CAST(@Data AS DATETIME2))
          )
    ORDER BY Horario;
END");

            
            migrationBuilder.Sql(@"
CREATE PROCEDURE usp_CadastrarAgendamento
    @BarbeiroId  INT,
    @ServicoId   INT,
    @DataHora    DATETIME2,
    @NomeCliente NVARCHAR(100),
    @Telefone    NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF DATEPART(MINUTE, @DataHora) <> 0
       OR DATEPART(SECOND, @DataHora) <> 0
       OR DATEPART(HOUR, @DataHora) < 8
       OR DATEPART(HOUR, @DataHora) > 17
    BEGIN
        RAISERROR(N'Horário inválido. Atendemos de 08h às 18h, em horas cheias.', 16, 1);
        RETURN;
    END

    IF @DataHora <= SYSDATETIME()
    BEGIN
        RAISERROR(N'Não é possível agendar em um horário que já passou.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM Servicos WHERE Id = @ServicoId)
    BEGIN
        RAISERROR(N'Serviço não encontrado.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM Barbeiros WHERE Id = @BarbeiroId AND Ativo = 1)
    BEGIN
        RAISERROR(N'Barbeiro não encontrado ou inativo.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM BarbeirosInativosDia
               WHERE BarbeiroId = @BarbeiroId AND CAST(Data AS DATE) = CAST(@DataHora AS DATE))
    BEGIN
        RAISERROR(N'O barbeiro não atende neste dia.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM Agendamentos
               WHERE BarbeiroId = @BarbeiroId AND DataHora = @DataHora AND Cancelado = 0)
    BEGIN
        RAISERROR(N'Horário indisponível para este barbeiro.', 16, 1);
        RETURN;
    END

    INSERT INTO Agendamentos (BarbeiroId, ServicoId, DataHora, NomeCliente, Telefone, Cancelado)
    VALUES (@BarbeiroId, @ServicoId, @DataHora, @NomeCliente, @Telefone, 0);

    SELECT SCOPE_IDENTITY() AS IdAgendamento;
END");

            
            migrationBuilder.Sql(@"
CREATE PROCEDURE usp_CancelarAgendamento
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Agendamentos
    SET Cancelado = 1
    WHERE Id = @Id AND Cancelado = 0;

    SELECT @@ROWCOUNT AS LinhasAfetadas;
END");

            
            migrationBuilder.Sql(@"
CREATE PROCEDURE usp_InativarBarbeiroNoDia
    @BarbeiroId INT,
    @Data       DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Barbeiros WHERE Id = @BarbeiroId)
    BEGIN
        RAISERROR(N'Barbeiro não encontrado.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM BarbeirosInativosDia
                   WHERE BarbeiroId = @BarbeiroId AND CAST(Data AS DATE) = @Data)
    BEGIN
        INSERT INTO BarbeirosInativosDia (BarbeiroId, Data)
        VALUES (@BarbeiroId, @Data);
    END

    SELECT COUNT(*) AS AgendamentosAindaMarcados
    FROM Agendamentos
    WHERE BarbeiroId = @BarbeiroId
      AND Cancelado = 0
      AND CAST(DataHora AS DATE) = @Data;
END");
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS usp_InativarBarbeiroNoDia");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS usp_CancelarAgendamento");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS usp_CadastrarAgendamento");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS usp_HorariosLivres");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS usp_AgendamentosDoDia");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_Agendamentos_NaoApagar");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_Agendamentos_Cancelamento");

            migrationBuilder.DropTable(
                name: "LogsCancelamento");

            migrationBuilder.DropIndex(
                name: "IX_Agendamentos_BarbeiroId_DataHora",
                table: "Agendamentos");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_BarbeiroId",
                table: "Agendamentos",
                column: "BarbeiroId");
        }
    }
}