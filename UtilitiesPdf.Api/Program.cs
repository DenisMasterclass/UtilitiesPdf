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
    var historicoService = scope.ServiceProvider.GetRequiredService<IImportacaoHistoricoService>();
    await historicoService.InicializarAsync(CancellationToken.None);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
