using Microsoft.AspNetCore.Http;
using UtilitiesPdf.Api.Models;

namespace UtilitiesPdf.Api.Services;

public interface IPdfImportService
{
    Task<ImportacaoPdfResultado> ProcessarArquivoAsync(IFormFile arquivo, CancellationToken cancellationToken);
    Task<ImportacaoPdfLoteResultado> ProcessarArquivosAsync(IEnumerable<IFormFile> arquivos, CancellationToken cancellationToken);
    Task<ImportacaoPdfLoteResultado> ProcessarPastaAsync(string caminhoPasta, CancellationToken cancellationToken);
}
