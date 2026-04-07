using Microsoft.AspNetCore.Http;
using UtilitiesPdf.Api.Models;
using Utils.Repositories.Enums;

namespace UtilitiesPdf.Api.Services;

public interface IPdfImportService
{
    Task<ImportacaoPdfResultado> ProcessarArquivoAsync(IFormFile arquivo, TipoPt tipoPt, CancellationToken cancellationToken);
    Task<ImportacaoPdfLoteResultado> ProcessarArquivosAsync(IEnumerable<IFormFile> arquivos, TipoPt tipoPt, CancellationToken cancellationToken);
    Task<ImportacaoPdfLoteResultado> ProcessarPastaAsync(string caminhoPasta, TipoPt tipoPt, CancellationToken cancellationToken);
}
