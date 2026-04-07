using Microsoft.AspNetCore.Mvc;
using UtilitiesPdf.Api.Models;
using UtilitiesPdf.Api.Services;
using Utils.Repositories.Enums;

namespace UtilitiesPdf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropostasController : ControllerBase
{
    private readonly IPdfImportService _pdfImportService;

    public PropostasController(IPdfImportService pdfImportService)
    {
        _pdfImportService = pdfImportService;
    }

    [HttpPost("importar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImportacaoPdfResultado), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportarPdfAsync(
        [FromForm] ImportarPropostaPdfRequest request, TipoPt tipoPt,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || request.Arquivo is null)
        {
            return BadRequest(ModelState);
        }

        var resultado = await _pdfImportService.ProcessarArquivoAsync(request.Arquivo, tipoPt, cancellationToken);

        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("importar-lote")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImportacaoPdfLoteResultado), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportarPdfsAsync(
        [FromForm] ImportarPropostasPdfRequest request, TipoPt tipoPt,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || request.Arquivos.Count == 0)
        {
            return BadRequest(ModelState);
        }

        var resultado = await _pdfImportService.ProcessarArquivosAsync(request.Arquivos, tipoPt, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("importar-pasta")]
    [ProducesResponseType(typeof(ImportacaoPdfLoteResultado), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportarPastaAsync(
        [FromBody] ImportarPropostasPorPastaRequest request, TipoPt tipoPt,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.CaminhoPasta))
        {
            return BadRequest(ModelState);
        }

        var resultado = await _pdfImportService.ProcessarPastaAsync(request.CaminhoPasta, tipoPt, cancellationToken);
        return Ok(resultado);
    }
}
