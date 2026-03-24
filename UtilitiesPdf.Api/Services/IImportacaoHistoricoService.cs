using UtilitiesPdf.Api.Models;

namespace UtilitiesPdf.Api.Services;

public interface IImportacaoHistoricoService
{
    Task InicializarAsync(CancellationToken cancellationToken);
    Task RegistrarAsync(ImportacaoHistoricoItem item, CancellationToken cancellationToken);
    Task<IReadOnlyList<ImportacaoHistoricoItem>> ListarAsync(DateTime? dataInicial, DateTime? dataFinal, CancellationToken cancellationToken);
}
