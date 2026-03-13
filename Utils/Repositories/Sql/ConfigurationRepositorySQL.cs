namespace Utils.Repositories.Sql
{
    public static class ConfigurationRepositorySQL
    {
        public const string SqlDelimaItFinanceiro = @"Server=tcp:delimait.database.windows.net,1433;Initial Catalog=Financeiro;Persist Security Info=False;User ID=denis;Password=Melzinh@01;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        
        public const string InsertSqlProjetosPropostaTecnica = @"SELECT COD_PARAMETRO AS Id,
                                                    NOM_PARAMETRO AS Name
                                                    FROM CONFIGURACAO_PARAMETRO P WITH(NOLOCK)
                                                    WHERE P.COD_MODULO = @ModuleId";
    }
}
