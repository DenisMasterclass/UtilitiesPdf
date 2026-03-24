namespace UtilitiesPdf.FollowUp.Services;

public class ImportacaoLoteResultadoViewModel
{
    public int TotalArquivos { get; set; }
    public int ArquivosProcessadosComSucesso { get; set; }
    public int ArquivosComErro { get; set; }
    public List<ImportacaoResultadoViewModel> Resultados { get; set; } = [];
}
