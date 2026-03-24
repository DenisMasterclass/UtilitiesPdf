using Microsoft.Data.SqlClient;
using UtilitiesPdf.Api.Models;

namespace UtilitiesPdf.Api.Services;

public class ImportacaoHistoricoService : IImportacaoHistoricoService
{
    private readonly string _connectionString;

    public ImportacaoHistoricoService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Financeiro")
            ?? throw new InvalidOperationException("Connection string 'Financeiro' nao configurada.");
    }

    public async Task InicializarAsync(CancellationToken cancellationToken)
    {
        const string sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ImportacaoArquivoHistorico' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ImportacaoArquivoHistorico
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ImportacaoArquivoHistorico PRIMARY KEY,
        NomeArquivo NVARCHAR(260) NOT NULL,
        Origem NVARCHAR(50) NOT NULL,
        Sucesso BIT NOT NULL,
        RegistrosAfetados INT NOT NULL CONSTRAINT DF_ImportacaoArquivoHistorico_RegistrosAfetados DEFAULT ((0)),
        Mensagem NVARCHAR(MAX) NOT NULL,
        DataProcessamento DATETIME2 NOT NULL CONSTRAINT DF_ImportacaoArquivoHistorico_DataProcessamento DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_ImportacaoArquivoHistorico_DataProcessamento
        ON dbo.ImportacaoArquivoHistorico (DataProcessamento DESC);
END";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RegistrarAsync(ImportacaoHistoricoItem item, CancellationToken cancellationToken)
    {
        const string sql = @"
INSERT INTO dbo.ImportacaoArquivoHistorico
(
    Id,
    NomeArquivo,
    Origem,
    Sucesso,
    RegistrosAfetados,
    Mensagem,
    DataProcessamento
)
VALUES
(
    @Id,
    @NomeArquivo,
    @Origem,
    @Sucesso,
    @RegistrosAfetados,
    @Mensagem,
    @DataProcessamento
);";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", item.Id);
        command.Parameters.AddWithValue("@NomeArquivo", item.NomeArquivo);
        command.Parameters.AddWithValue("@Origem", item.Origem);
        command.Parameters.AddWithValue("@Sucesso", item.Sucesso);
        command.Parameters.AddWithValue("@RegistrosAfetados", item.RegistrosAfetados);
        command.Parameters.AddWithValue("@Mensagem", item.Mensagem);
        command.Parameters.AddWithValue("@DataProcessamento", item.DataProcessamento);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ImportacaoHistoricoItem>> ListarAsync(DateTime? dataInicial, DateTime? dataFinal, CancellationToken cancellationToken)
    {
        var resultados = new List<ImportacaoHistoricoItem>();
        const string sql = @"
SELECT Id, NomeArquivo, Origem, Sucesso, RegistrosAfetados, Mensagem, DataProcessamento
FROM dbo.ImportacaoArquivoHistorico
WHERE (@DataInicial IS NULL OR DataProcessamento >= @DataInicial)
  AND (@DataFinal IS NULL OR DataProcessamento < @DataFinal)
ORDER BY DataProcessamento DESC;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DataInicial", (object?)dataInicial ?? DBNull.Value);
        command.Parameters.AddWithValue("@DataFinal", (object?)dataFinal?.Date.AddDays(1) ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            resultados.Add(new ImportacaoHistoricoItem
            {
                Id = reader.GetGuid(0),
                NomeArquivo = reader.GetString(1),
                Origem = reader.GetString(2),
                Sucesso = reader.GetBoolean(3),
                RegistrosAfetados = reader.GetInt32(4),
                Mensagem = reader.GetString(5),
                DataProcessamento = reader.GetDateTime(6)
            });
        }

        return resultados;
    }
}
