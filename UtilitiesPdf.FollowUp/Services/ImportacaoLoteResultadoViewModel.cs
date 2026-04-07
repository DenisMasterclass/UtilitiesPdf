namespace UtilitiesPdf.FollowUp.Services;

public class ImportacaoLoteResultadoViewModel
{
    public int TotalArquivos { get; set; }
    public int ArquivosProcessadosComSucesso { get; set; }
    public int ArquivosComErro { get; set; }
    public int? TipoPt { get; set; }
    public string TipoPtLabel { get; set; } = string.Empty;
    public List<ImportacaoResultadoViewModel> Resultados { get; set; } = [];
}
