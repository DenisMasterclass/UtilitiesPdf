namespace Utils.Repositories.Sql
{
    public static class ConfigurationRepositorySQL
    {
        public const string SqlDelimaItFinanceiro = @"Server=tcp:delimait.database.windows.net,1433;Initial Catalog=Financeiro;Persist Security Info=False;User ID=access;Password=PortoBank@01;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public const string InsertSqlPropostasPropostaTecnica = @"SELECT COD_PARAMETRO AS Id,
                                                    NOM_PARAMETRO AS Name
                                                    FROM CONFIGURACAO_PARAMETRO P WITH(NOLOCK)
                                                    WHERE P.COD_MODULO = @ModuleId";

        public const string InsertSqlProjeto = @"
INSERT INTO dbo.Projeto
(
    IdProjeto,
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
    @IdProjeto,
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

        public const string InsertSqlTipoProjeto = @"
INSERT INTO dbo.TipoProjeto
(
    Id,
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

        public const string InsertSqlProjetoTipoProjeto = @"
INSERT INTO dbo.ProjetoTipoProjeto
(
    IdProjeto,
    IdTipoProjeto
)
VALUES
(
    @IdProjeto,
    @IdTipoProjeto
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

        public const string InsertSqlProjetoPacote = @"
INSERT INTO dbo.ProjetoPacote
(
    IdProjeto,
    IdPacote
)
VALUES
(
    @IdProjeto,
    @IdPacote
);";
    }
}
