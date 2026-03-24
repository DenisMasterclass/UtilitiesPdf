namespace UtilitiesPdf.Api.Models;

public class ImportacaoHistoricoItem
{
    public Guid Id { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public bool Sucesso { get; set; }
    public int RegistrosAfetados { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public DateTime DataProcessamento { get; set; }
}
