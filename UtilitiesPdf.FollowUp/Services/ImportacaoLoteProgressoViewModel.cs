namespace UtilitiesPdf.FollowUp.Services;

public class ImportacaoLoteProgressoViewModel
{
    public int TotalArquivos { get; set; }
    public int ArquivosProcessados { get; set; }
    public string NomeArquivoAtual { get; set; } = string.Empty;
    public int Percentual => TotalArquivos == 0 ? 0 : (int)Math.Round((double)ArquivosProcessados / TotalArquivos * 100);
}
