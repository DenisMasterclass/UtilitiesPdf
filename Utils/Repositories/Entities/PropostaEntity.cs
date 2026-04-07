using System.Text;
using Utils.Repositories.Enums;

namespace Utils.Repositories.Entities
{
    public class PropostaEntity
    {
        public PropostaEntity()
        {
        }
        public Guid IdProposta { get; set; } = Guid.NewGuid();
        public Guid IdTipoProposta { get; set; } = Guid.Empty;
        public string IdDocusign { get; set; } = string.Empty;
        public TipoPt TipoPt { get; set; } = TipoPt.Alocação;
        public string VersaoProposta { get; set; } = string.Empty;
        public string Vigencia { get; set; } = string.Empty;
        public string Fornecedor { get; set; } = string.Empty;
        public string Preposto { get; set; } = string.Empty;
        public string EmailPreposto { get; set; } = string.Empty;
        public string NumeroProposta { get; set; } = string.Empty;
        public string NumeroPropostaComercial { get; set; } = string.Empty;
        public TipoPropostaEntity TipoProposta { get; set; } = new();
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
