namespace Utils.Repositories.Entities
{
    public class PacoteEntity
    {
        public PacoteEntity()
        {
        }
        public Guid IdPacote { get; set; } = Guid.NewGuid();
        public Guid IdProposta { get; set; } = Guid.Empty;
        public string Pacote { get; set; } = default;
        public string Horas { get; set; } = default;
        public string DataIni { get; set; } = default;
        public string DataFim { get; set; } = default;
        public string Perfil { get; set; } = default;
    }
}
