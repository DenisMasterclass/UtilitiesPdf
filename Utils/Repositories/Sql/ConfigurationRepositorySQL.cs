namespace Utils.Repositories.Sql
{
    public static class ConfigurationRepositorySQL
    {
        public const string SqlDelimaItFinanceiro = @"Server=tcp:delimait01.database.windows.net,1433;Initial Catalog=Governanca;Persist Security Info=False;User ID=denis;Password=Melzinh@01;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public const string InsertSqlProposta = @"
INSERT INTO dbo.Proposta
(
    IdProposta,
    IdDocusign,
    TipoProposta,
    VersaoProposta,
    Vigencia,
    Fornecedor,
    Preposto,
    EmailPreposto,
    NumeroProposta,
    NumeroPropostaComercial,
    HorasTotais,
    LocalTrabalho,
    Premissas,
    DentroEscopo,
    ForaEscopo,
    DocumentoComplementar,
    Aceite
)
VALUES
(
    @IdProposta,
    @IdDocusign,
    @TipoProposta,
    @VersaoProposta,
    @Vigencia,
    @Fornecedor,
    @Preposto,
    @EmailPreposto,
    @NumeroProposta,
    @NumeroPropostaComercial,
    @HorasTotais,
    @LocalTrabalho,
    @Premissas,
    @DentroEscopo,
    @ForaEscopo,
    @DocumentoComplementar,
    @Aceite
);";

        public const string InsertSqlTipoProposta = @"
INSERT INTO dbo.TipoProposta
(
    IdTipoProposta,
    Analise,
    Requisitos,
    AnaliseProgramacao,
    Testes,
    Programacao,
    Etl,
    Arquitetura,
    EspecificacaoExecucaoTestes
)
VALUES
(
    @Id,
    @Analise,
    @Requisitos,
    @AnaliseProgramacao,
    @Testes,
    @Programacao,
    @Etl,
    @Arquitetura,
    @EspecificacaoExecucaoTestes
);";

        public const string InsertSqlPropostaTipoProposta = @"
INSERT INTO dbo.PropostaTipoProposta
(
    IdProposta,
    IdTipoProposta
)
VALUES
(
    @IdProposta,
    @IdTipoProposta
);";

        public const string InsertSqlPacote = @"
INSERT INTO dbo.Pacote
(
    Id,
    IdPacote,
    Horas,
    DataIni,
    DataFim
)
VALUES
(
    @Id,
    @IdPacote,
    @Horas,
    @DataIni,
    @DataFim
);";

        public const string InsertSqlPropostaPacote = @"
INSERT INTO dbo.PropostaPacote
(
    IdProposta,
    IdPacote
)
VALUES
(
    @IdProposta,
    @IdPacote
);";
    }
}
