using System.Net.Http.Json;

namespace UtilitiesPdf.FollowUp.Services;

public class HistoricoImportacaoClient
{
    private readonly HttpClient _httpClient;

    public HistoricoImportacaoClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ImportacaoHistoricoItemViewModel>> ListarAsync(DateTime? dataInicial, DateTime? dataFinal, CancellationToken cancellationToken)
    {
        var query = new List<string>();

        if (dataInicial.HasValue)
        {
            query.Add($"dataInicial={Uri.EscapeDataString(dataInicial.Value.ToString("yyyy-MM-dd"))}");
        }

        if (dataFinal.HasValue)
        {
            query.Add($"dataFinal={Uri.EscapeDataString(dataFinal.Value.ToString("yyyy-MM-dd"))}");
        }

        var url = "api/importacoes/historico";
        if (query.Count > 0)
        {
            url += "?" + string.Join("&", query);
        }

        var response = await _httpClient.GetFromJsonAsync<List<ImportacaoHistoricoItemViewModel>>(url, cancellationToken);
        return response ?? [];
    }
}
