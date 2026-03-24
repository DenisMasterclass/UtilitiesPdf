using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;

namespace UtilitiesPdf.FollowUp.Services;

public class ImportacaoApiClient
{
    private const long MaxFileSizeBytes = 50L * 1024 * 1024;
    private readonly HttpClient _httpClient;

    public ImportacaoApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ImportacaoResultadoViewModel?> ImportarArquivoAsync(IBrowserFile arquivo, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = await CriarConteudoArquivoAsync(arquivo, cancellationToken);
        content.Add(fileContent, "Arquivo", arquivo.Name);

        using var response = await _httpClient.PostAsync("api/Propostas/importar", content, cancellationToken);
        return await LerRespostaAsync<ImportacaoResultadoViewModel>(response, cancellationToken);
    }

    public async Task<ImportacaoLoteResultadoViewModel> ImportarArquivosAsync(
        IReadOnlyList<IBrowserFile> arquivos,
        IProgress<ImportacaoLoteProgressoViewModel>? progresso,
        CancellationToken cancellationToken)
    {
        var resultados = new List<ImportacaoResultadoViewModel>();
        var totalArquivos = arquivos.Count;

        for (var indice = 0; indice < arquivos.Count; indice++)
        {
            var arquivo = arquivos[indice];
            progresso?.Report(new ImportacaoLoteProgressoViewModel
            {
                TotalArquivos = totalArquivos,
                ArquivosProcessados = indice,
                NomeArquivoAtual = arquivo.Name
            });

            try
            {
                var resultado = await ImportarArquivoAsync(arquivo, cancellationToken);
                if (resultado is not null)
                {
                    resultados.Add(resultado);
                }
            }
            catch (Exception ex)
            {
                resultados.Add(new ImportacaoResultadoViewModel
                {
                    NomeArquivo = arquivo.Name,
                    Sucesso = false,
                    Mensagem = ex.Message
                });
            }

            progresso?.Report(new ImportacaoLoteProgressoViewModel
            {
                TotalArquivos = totalArquivos,
                ArquivosProcessados = indice + 1,
                NomeArquivoAtual = arquivo.Name
            });
        }

        return new ImportacaoLoteResultadoViewModel
        {
            TotalArquivos = resultados.Count,
            ArquivosProcessadosComSucesso = resultados.Count(x => x.Sucesso),
            ArquivosComErro = resultados.Count(x => !x.Sucesso),
            Resultados = resultados
        };
    }

    public async Task<ImportacaoLoteResultadoViewModel?> ImportarPastaAsync(string caminhoPasta, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/Propostas/importar-pasta", new { caminhoPasta }, cancellationToken);
        return await LerRespostaAsync<ImportacaoLoteResultadoViewModel>(response, cancellationToken);
    }

    private static async Task<ByteArrayContent> CriarConteudoArquivoAsync(IBrowserFile arquivo, CancellationToken cancellationToken)
    {
        await using var fileStream = arquivo.OpenReadStream(MaxFileSizeBytes, cancellationToken);
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, cancellationToken);

        var fileContent = new ByteArrayContent(memoryStream.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(arquivo.ContentType) ? "application/pdf" : arquivo.ContentType);
        return fileContent;
    }

    private static async Task<T?> LerRespostaAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var corpo = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Erro HTTP {(int)response.StatusCode}: {corpo}");
        }

        if (string.IsNullOrWhiteSpace(corpo))
        {
            return default;
        }

        return System.Text.Json.JsonSerializer.Deserialize<T>(corpo, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Resposta da API vazia ou invalida.");
    }
}
