namespace UtilitiesPdf.Api.Models;

public class ImportacaoPdfLoteResultado
{
    public int TotalArquivos { get; set; }
    public int ArquivosProcessadosComSucesso { get; set; }
    public int ArquivosComErro { get; set; }
    public List<ImportacaoPdfResultado> Resultados { get; set; } = [];
}
