namespace Utils.Repositories.Entities
{
    public class TipoProjetoEntity
    {
        public TipoProjetoEntity()
        {
        }
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool Analise { get; set; } = false;
        public bool Requisitos { get; set; } = false;
        public bool AnaliseProgramacao { get; set; } = false;
        public bool Testes { get; set; } = false;
        public bool Programacao { get; set; } = false;
        public bool Etl { get; set; } = false;
        public  bool Arquitetura { get; set; } = false;
        public bool EspecificacaoExecucaoTestes { get; set; } = false;

    }
}
