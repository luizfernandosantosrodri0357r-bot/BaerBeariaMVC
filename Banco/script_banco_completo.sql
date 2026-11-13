-- Garantir que a tabela de logs existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LogsCancelamento')
BEGIN
    CREATE TABLE [LogsCancelamento] (
        [Id] int NOT NULL IDENTITY,
        [AgendamentoId] int NOT NULL,
        [NomeCliente] nvarchar(100) NOT NULL,
        [DataHoraAgendamento] datetime2 NOT NULL,
        [DataCancelamento] datetime2 NOT NULL,
        CONSTRAINT [PK_LogsCancelamento] PRIMARY KEY ([Id])
    );
END
GO

-- Índice exclusivo condicional
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Agendamentos_BarbeiroId_DataHora')
BEGIN
    CREATE UNIQUE INDEX [IX_Agendamentos_BarbeiroId_DataHora] ON [Agendamentos] ([BarbeiroId], [DataHora]) WHERE [Cancelado] = 0;
END
GO

-- Trigger de Cancelamento (Precisa estar no início do lote)
CREATE OR ALTER TRIGGER trg_Agendamentos_Cancelamento
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
END
GO

-- Trigger de Bloqueio de Delete
CREATE OR ALTER TRIGGER trg_Agendamentos_NaoApagar
ON Agendamentos
INSTEAD OF DELETE
AS
BEGIN
    RAISERROR(N'Agendamentos não podem ser apagados. Use o cancelamento.', 16, 1);
END
GO

-- Procedure: Agendamentos do Dia
CREATE OR ALTER PROCEDURE usp_AgendamentosDoDia
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
END
GO

-- Procedure: Horários Livres
CREATE OR ALTER PROCEDURE usp_HorariosLivres
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
END
GO

-- Procedure: Cadastrar Agendamento
CREATE OR ALTER PROCEDURE usp_CadastrarAgendamento
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
END
GO

-- Procedure: Cancelar Agendamento
CREATE OR ALTER PROCEDURE usp_CancelarAgendamento
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Agendamentos
    SET Cancelado = 1
    WHERE Id = @Id AND Cancelado = 0;

    SELECT @@ROWCOUNT AS LinhasAfetadas;
END
GO

-- Procedure: Inativar Barbeiro no Dia
CREATE OR ALTER PROCEDURE usp_InativarBarbeiroNoDia
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
END
GO

-- Registrar na tabela de histórico de migrations para o EF Core não tentar rodar de novo
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105014521_BancoAvancado'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261105014521_BancoAvancado', N'10.0.13');
END;
GO