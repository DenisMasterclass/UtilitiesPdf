using Utils.Repositories.Enums;

namespace UtilitiesPdf.Api.Models;

public class ImportacaoPdfResultado
{
    public string NomeArquivo { get; set; } = string.Empty;
    public bool Sucesso { get; set; }
    public int RegistrosAfetados { get; set; }
    public string? Mensagem { get; set; }
    public TipoPt? TipoPt { get; set; }
    public string TipoPtLabel { get; set; } = string.Empty;
}
