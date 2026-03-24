using Microsoft.AspNetCore.Mvc;
using UtilitiesPdf.Api.Models;
using UtilitiesPdf.Api.Services;

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
        [FromForm] ImportarPropostaPdfRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || request.Arquivo is null)
        {
            return BadRequest(ModelState);
        }

        var resultado = await _pdfImportService.ProcessarArquivoAsync(request.Arquivo, cancellationToken);

        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("importar-lote")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImportacaoPdfLoteResultado), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportarPdfsAsync(
        [FromForm] ImportarPropostasPdfRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || request.Arquivos.Count == 0)
        {
            return BadRequest(ModelState);
        }

        var resultado = await _pdfImportService.ProcessarArquivosAsync(request.Arquivos, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("importar-pasta")]
    [ProducesResponseType(typeof(ImportacaoPdfLoteResultado), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportarPastaAsync(
        [FromBody] ImportarPropostasPorPastaRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.CaminhoPasta))
        {
            return BadRequest(ModelState);
        }

        var resultado = await _pdfImportService.ProcessarPastaAsync(request.CaminhoPasta, cancellationToken);
        return Ok(resultado);
    }
}
