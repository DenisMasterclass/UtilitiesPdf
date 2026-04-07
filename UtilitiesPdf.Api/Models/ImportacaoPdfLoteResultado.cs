using Utils.Repositories.Enums;

namespace UtilitiesPdf.Api.Models;

public class ImportacaoPdfLoteResultado
{
    public int TotalArquivos { get; set; }
    public int ArquivosProcessadosComSucesso { get; set; }
    public int ArquivosComErro { get; set; }
    public TipoPt? TipoPt { get; set; }
    public string TipoPtLabel { get; set; } = string.Empty;
    public List<ImportacaoPdfResultado> Resultados { get; set; } = [];
}
