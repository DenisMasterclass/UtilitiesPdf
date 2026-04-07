using Utils.DependencyInjection;
using Utils.Repositories.Sql;
using Utils.Shared.Repository;
using UtilitiesPdf.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddPdfReaders();

var connectionString = builder.Configuration.GetConnectionString("Financeiro")
                      ?? ConfigurationRepositorySQL.SqlDelimaItFinanceiro;

builder.Services.AddSqlServerDeLimaIt(connectionString, "30");
builder.Services.AddScoped<IPdfImportService, PdfImportService>();
builder.Services.AddSingleton<IImportacaoLogService, ImportacaoLogService>();
builder.Services.AddSingleton<IImportacaoHistoricoService, ImportacaoHistoricoService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var historicoService = scope.ServiceProvider.GetRequiredService<IImportacaoHistoricoService>();

    try
    {
        await historicoService.InicializarAsync(CancellationToken.None);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Nao foi possivel inicializar o historico de importacao no startup. A API sera iniciada e uma nova tentativa ocorrera durante o uso.");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
