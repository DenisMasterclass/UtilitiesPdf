using System.Text;

namespace Utils.Repositories.Entities
{
    public class ProjetoEntity
    {
        public ProjetoEntity()
        {
        }
        public Guid IdProjeto { get; set; } = new Guid();
        public string IdDocusign { get; set; } = string.Empty;
        public string TipoProposta { get; set; } = string.Empty;
        public string VersaoProposta { get; set; } = string.Empty;
        public string Vigencia { get; set; } = string.Empty;
        public string Fornecedor { get; set; } = string.Empty;
        public string Preposto { get; set; } = string.Empty;
        public string EmailPreposto { get; set; } = string.Empty;
        public string NumeroProposta { get; set; } = string.Empty;
        public string NumeroPropostaComercial { get; set; } = string.Empty;
        public TipoProjetoEntity TipoProjeto { get; set; } = new();
        public List<PacoteEntity> Pacotes { get; set; } = new();
        public decimal HorasTotais { get; set; } = default;
        public string LocalTrabalho { get; set; } = string.Empty;
        public string Premissas { get; set; } = string.Empty;
        public string DentroEscopo { get; set; } = string.Empty;
        public string ForaEscopo { get; set; } = string.Empty;
        public string DocumentoComplementar { get; set; } = string.Empty;
        public StringBuilder Aceite { get; set; } = new StringBuilder();

    }
}
