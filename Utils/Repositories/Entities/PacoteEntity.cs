namespace Utils.Repositories.Entities
{
    public class PacoteEntity
    {
        public PacoteEntity()
        {
        }
        public Guid Id { get; set; } = Guid.NewGuid();
        public int? IdPacote { get; set; } = default;
        public decimal Horas { get; set; } = default;
        public DateOnly DataIni { get; set; } = default;
        public DateOnly DataFim { get; set; } = default;
    }
}
