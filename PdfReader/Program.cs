using System.Text;
using System.Text.Json;
using Utils.Pdf;
using static Utils.Pdf.JsonFields;
using Microsoft.Extensions.DependencyInjection;
using Utils.DependencyInjection;
using Utils.Shared.Repository;
using static Utils.Repositories.Sql.ConfigurationRepositorySQL;
using Utils.Repositories;
using Utils.Repositories.Enums;



namespace PdfReader
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddPdfReaders();
            services.AddSqlServerDeLimaIt(SqlDelimaItFinanceiro, "30");
            var provider = services.BuildServiceProvider();
            var repository = provider.GetRequiredService<IRepository>();

            var sb = new StringBuilder();
            string Propostas = @"D:\\PortoBank\\Proposta Técnica\\Propostas";

            foreach (string arquivo in Directory.GetFiles(Propostas, "*.pdf"))
            {
                Console.WriteLine($"Lendo: {System.IO.Path.GetFileName(arquivo)}");
                sb = sb.PdfToTxt(Propostas + "\\" + $"{System.IO.Path.GetFileName(arquivo)}");
                var Proposta = await repository.CamposPropostas(sb, TipoPt.Projeto);
                await repository.InserirPropostaAsync(Proposta);

                Console.Write(sb.ToString());
            }

            // Exemplo de JSON (pode vir de um arquivo com File.ReadAllText("caminho.json"))
            string jsonString = @"
        {
            ""usuario"": {
                ""nome"": ""Ana"",
                ""id"": 123,
                ""preferencias"": {
                    ""tema"": ""escuro"",
                    ""notificacoes"": true
                }
            },
            ""historico"": [
                { ""data"": ""2023-10-01"", ""acao"": ""login"" },
                { ""data"": ""2023-10-02"", ""acao"": ""logout"" }
            ]
        }";

            try
            {
                // Parseia o JSON para um documento navegável
                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    // Inicia a navegação a partir da raiz
                    PercorrerElemento(doc.RootElement);
                }
            }
            catch (JsonException e)
            {
                //Console.WriteLine($"Erro ao ler JSON: {e.Message}");
            }
        }

        /// <summary>
        /// Método recursivo que analisa o tipo do elemento e extrai os nomes
        /// </summary>

    }

}
