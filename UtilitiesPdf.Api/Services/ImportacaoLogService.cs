using System.Text;

namespace UtilitiesPdf.Api.Services;

public class ImportacaoLogService : IImportacaoLogService
{
    private readonly string _diretorioLogs;

    public ImportacaoLogService(IConfiguration configuration)
    {
        var diretorioConfigurado = configuration["ImportacaoPdf:DiretorioLogs"];
        _diretorioLogs = Path.IsPathRooted(diretorioConfigurado)
            ? diretorioConfigurado
            : Path.Combine(AppContext.BaseDirectory, diretorioConfigurado ?? "Logs");
    }

    public async Task RegistrarErroAsync(string nomeArquivo, Exception exception, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_diretorioLogs);

        var caminhoLog = Path.Combine(_diretorioLogs, $"importacao-pdf-{DateTime.UtcNow:yyyyMMdd}.log");
        var builder = new StringBuilder();
        builder.AppendLine($"DataUtc: {DateTime.UtcNow:O}");
        builder.AppendLine($"Arquivo: {nomeArquivo}");
        builder.AppendLine($"Erro: {exception.Message}");
        builder.AppendLine(exception.ToString());
        builder.AppendLine(new string('-', 80));

        await File.AppendAllTextAsync(caminhoLog, builder.ToString(), cancellationToken);
    }
}
