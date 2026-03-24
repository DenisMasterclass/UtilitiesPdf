namespace UtilitiesPdf.Api.Services;

public interface IImportacaoLogService
{
    Task RegistrarErroAsync(string nomeArquivo, Exception exception, CancellationToken cancellationToken);
}
