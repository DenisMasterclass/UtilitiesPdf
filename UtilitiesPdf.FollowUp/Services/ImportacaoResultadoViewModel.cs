namespace UtilitiesPdf.FollowUp.Services;

public class ImportacaoResultadoViewModel
{
    public string NomeArquivo { get; set; } = string.Empty;
    public bool Sucesso { get; set; }
    public int RegistrosAfetados { get; set; }
    public string? Mensagem { get; set; }
}
