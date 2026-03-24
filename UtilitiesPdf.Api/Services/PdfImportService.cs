using System.Text;
using Utils.Pdf;
using Utils.Repositories;
using UtilitiesPdf.Api.Models;

namespace UtilitiesPdf.Api.Services;

public class PdfImportService : IPdfImportService
{
    private readonly IRepository _repository;
    private readonly IImportacaoLogService _logService;
    private readonly IImportacaoHistoricoService _historicoService;
    private readonly ILogger<PdfImportService> _logger;

    public PdfImportService(
        IRepository repository,
        IImportacaoLogService logService,
        IImportacaoHistoricoService historicoService,
        ILogger<PdfImportService> logger)
    {
        _repository = repository;
        _logService = logService;
        _historicoService = historicoService;
        _logger = logger;
    }

    public async Task<ImportacaoPdfResultado> ProcessarArquivoAsync(IFormFile arquivo, CancellationToken cancellationToken)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            return new ImportacaoPdfResultado
            {
                NomeArquivo = arquivo?.FileName ?? string.Empty,
                Sucesso = false,
                Mensagem = "Arquivo PDF nao informado ou vazio."
            };
        }

        var caminhoTemporario = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}-{Path.GetFileName(arquivo.FileName)}");

        try
        {
            await using (var stream = File.Create(caminhoTemporario))
            {
                await arquivo.CopyToAsync(stream, cancellationToken);
            }

            return await ProcessarArquivoDoDiscoAsync(caminhoTemporario, arquivo.FileName, "Upload", cancellationToken);
        }
        finally
        {
            if (File.Exists(caminhoTemporario))
            {
                File.Delete(caminhoTemporario);
            }
        }
    }

    public async Task<ImportacaoPdfLoteResultado> ProcessarArquivosAsync(IEnumerable<IFormFile> arquivos, CancellationToken cancellationToken)
    {
        var resultados = new List<ImportacaoPdfResultado>();

        foreach (var arquivo in arquivos)
        {
            var resultado = await ProcessarArquivoAsync(arquivo, cancellationToken);
            resultados.Add(resultado);
        }

        return CriarResultadoLote(resultados);
    }

    public async Task<ImportacaoPdfLoteResultado> ProcessarPastaAsync(string caminhoPasta, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(caminhoPasta))
        {
            return CriarResultadoLote(
            [
                new ImportacaoPdfResultado
                {
                    NomeArquivo = string.Empty,
                    Sucesso = false,
                    Mensagem = "Caminho da pasta nao informado."
                }
            ]);
        }

        if (!Directory.Exists(caminhoPasta))
        {
            return CriarResultadoLote(
            [
                new ImportacaoPdfResultado
                {
                    NomeArquivo = caminhoPasta,
                    Sucesso = false,
                    Mensagem = "Pasta nao encontrada."
                }
            ]);
        }

        var resultados = new List<ImportacaoPdfResultado>();
        var caminhosArquivos = Directory.GetFiles(caminhoPasta, "*.pdf", SearchOption.TopDirectoryOnly);

        foreach (var caminhoArquivo in caminhosArquivos)
        {
            var resultado = await ProcessarArquivoDoDiscoAsync(caminhoArquivo, Path.GetFileName(caminhoArquivo), "Pasta", cancellationToken);
            resultados.Add(resultado);
        }

        return CriarResultadoLote(resultados);
    }

    private async Task<ImportacaoPdfResultado> ProcessarArquivoDoDiscoAsync(
        string caminhoArquivo,
        string nomeArquivo,
        string origem,
        CancellationToken cancellationToken)
    {
        ImportacaoPdfResultado resultado;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var conteudoPdf = new StringBuilder().PdfToTxt(caminhoArquivo);
            var Proposta = await _repository.CamposPropostas(conteudoPdf);
            var registrosAfetados = await _repository.InserirPropostaAsync(Proposta);

            resultado = new ImportacaoPdfResultado
            {
                NomeArquivo = nomeArquivo,
                Sucesso = true,
                RegistrosAfetados = registrosAfetados,
                Mensagem = "Arquivo processado com sucesso."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar o arquivo {NomeArquivo}", nomeArquivo);
            await _logService.RegistrarErroAsync(nomeArquivo, ex, cancellationToken);

            resultado = new ImportacaoPdfResultado
            {
                NomeArquivo = nomeArquivo,
                Sucesso = false,
                Mensagem = ex.Message
            };
        }

        await _historicoService.RegistrarAsync(new ImportacaoHistoricoItem
        {
            Id = Guid.NewGuid(),
            NomeArquivo = resultado.NomeArquivo,
            Origem = origem,
            Sucesso = resultado.Sucesso,
            RegistrosAfetados = resultado.RegistrosAfetados,
            Mensagem = resultado.Mensagem ?? string.Empty,
            DataProcessamento = DateTime.Now
        }, cancellationToken);

        return resultado;
    }

    private static ImportacaoPdfLoteResultado CriarResultadoLote(List<ImportacaoPdfResultado> resultados)
    {
        return new ImportacaoPdfLoteResultado
        {
            TotalArquivos = resultados.Count,
            ArquivosProcessadosComSucesso = resultados.Count(x => x.Sucesso),
            ArquivosComErro = resultados.Count(x => !x.Sucesso),
            Resultados = resultados
        };
    }
}
