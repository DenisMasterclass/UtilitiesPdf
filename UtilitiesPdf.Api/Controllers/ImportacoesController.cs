using Microsoft.AspNetCore.Mvc;
using UtilitiesPdf.Api.Models;
using UtilitiesPdf.Api.Services;

namespace UtilitiesPdf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportacoesController : ControllerBase
{
    private readonly IImportacaoHistoricoService _historicoService;

    public ImportacoesController(IImportacaoHistoricoService historicoService)
    {
        _historicoService = historicoService;
    }

    [HttpGet("historico")]
    [ProducesResponseType(typeof(IReadOnlyList<ImportacaoHistoricoItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarHistoricoAsync(
        [FromQuery] DateTime? dataInicial,
        [FromQuery] DateTime? dataFinal,
        CancellationToken cancellationToken)
    {
        var historico = await _historicoService.ListarAsync(dataInicial, dataFinal, cancellationToken);
        return Ok(historico);
    }
}
