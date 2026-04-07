using Microsoft.Data.SqlClient;
using UtilitiesPdf.Api.Models;

namespace UtilitiesPdf.Api.Services;

public class ImportacaoHistoricoService : IImportacaoHistoricoService
{
    private readonly string _connectionString;
    private readonly ILogger<ImportacaoHistoricoService> _logger;
    private volatile bool _estruturaInicializada;

    public ImportacaoHistoricoService(IConfiguration configuration, ILogger<ImportacaoHistoricoService> logger)
    {
        _logger = logger;
        _connectionString = AjustarConnectionString(
            configuration.GetConnectionString("Financeiro")
            ?? throw new InvalidOperationException("Connection string 'Financeiro' nao configurada."));
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
        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = 60
        };
        await command.ExecuteNonQueryAsync(cancellationToken);
        _estruturaInicializada = true;
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

        try
        {
            await GarantirEstruturaAsync(cancellationToken);

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(sql, connection)
            {
                CommandTimeout = 60
            };
            command.Parameters.AddWithValue("@Id", item.Id);
            command.Parameters.AddWithValue("@NomeArquivo", item.NomeArquivo);
            command.Parameters.AddWithValue("@Origem", item.Origem);
            command.Parameters.AddWithValue("@Sucesso", item.Sucesso);
            command.Parameters.AddWithValue("@RegistrosAfetados", item.RegistrosAfetados);
            command.Parameters.AddWithValue("@Mensagem", item.Mensagem);
            command.Parameters.AddWithValue("@DataProcessamento", item.DataProcessamento);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is SqlException or TimeoutException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Nao foi possivel registrar o historico de importacao do arquivo {NomeArquivo}.", item.NomeArquivo);
        }
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

        try
        {
            await GarantirEstruturaAsync(cancellationToken);

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(sql, connection)
            {
                CommandTimeout = 60
            };
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
        }
        catch (Exception ex) when (ex is SqlException or TimeoutException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Nao foi possivel consultar o historico de importacao.");
        }

        return resultados;
    }

    private async Task GarantirEstruturaAsync(CancellationToken cancellationToken)
    {
        if (_estruturaInicializada)
        {
            return;
        }

        await InicializarAsync(cancellationToken);
    }

    private static string AjustarConnectionString(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            ConnectTimeout = Math.Max(new SqlConnectionStringBuilder(connectionString).ConnectTimeout, 60),
            ConnectRetryCount = Math.Max(new SqlConnectionStringBuilder(connectionString).ConnectRetryCount, 3),
            ConnectRetryInterval = Math.Max(new SqlConnectionStringBuilder(connectionString).ConnectRetryInterval, 10)
        };

        return builder.ConnectionString;
    }
}
