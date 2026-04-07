namespace Utils.Repositories.Entities
{
    public class TipoPropostaEntity
    {
        public TipoPropostaEntity()
        {
        }
        public Guid IdTipoProposta { get; set; } = Guid.NewGuid();
        public bool Analise { get; set; }
        public bool Requisitos { get; set; }
        public bool AnaliseProgramacao { get; set; }
        public bool Testes { get; set; }
        public bool Programacao { get; set; }
        public bool Etl { get; set; }
        public  bool Arquitetura { get; set; }
        public bool EspecificacaoExecucaoTestes { get; set; }
        public Guid IdProposta { get; set; }

    }
}
