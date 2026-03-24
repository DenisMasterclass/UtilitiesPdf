using Dapper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Utils.Repositories.Entities;
using Utils.Shared;
using static Utils.Repositories.Sql.ConfigurationRepositorySQL;

namespace Utils.Repositories
{
    public class Repository : IRepository
    {
        private readonly DataContext _context;

        public Repository(DataContext context)
        {
            _context = context;
        }

        public string ExtrairValor(string texto, string marcador, string posicao = "abaixo")
        {
            var linhas = texto.Split('\n');
            for (int i = 0; i < linhas.Length; i++)
            {
                if (linhas[i].Contains(marcador))
                {
                    switch (posicao.ToLower())
                    {
                        case "abaixo":
                            return (i < linhas.Length - 1) ? linhas[i + 1].Trim() : "";
                        case "acima":
                            return (i > 0) ? linhas[i - 1].Trim() : "";
                        case "lado":
                            return linhas[i].Replace(marcador, "").Trim();
                    }
                }
            }
            return "";
        }

        public string ExtrairBloco(string texto, string marcadorInicio, string marcadorFim)
        {
            string padrao = marcadorInicio + @"([\s\S]*?)" + marcadorFim;

            var match = Regex.Match(texto, padrao, RegexOptions.Multiline | RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return string.Empty;
        }

        public async Task<PropostaEntity> CamposPropostas(StringBuilder sb)
        {
            PropostaEntity Proposta = new PropostaEntity();
            PacoteEntity pacote = new PacoteEntity();

            Proposta.IdProposta = Guid.NewGuid();
            Proposta.IdDocusign = ExtrairValor(sb.ToString(), "Docusign Envelope ID:", "lado");
            Proposta.TipoPt = ExtrairValor(sb.ToString(), "Identificador", "abaixo");
            Proposta.VersaoProposta = ExtrairValor(sb.ToString(), "Vigência Atualização Publicação Versão", "abaixo");
            Proposta.Vigencia = ExtrairBloco(sb.ToString(), "GESTAO_FORNEC_103", "GESTÃO DE FORNECEDORES DE TI");
            Proposta.Fornecedor = ExtrairValor(sb.ToString(), "FORNECEDOR:", "abaixo");
            Proposta.Preposto = ExtrairValor(sb.ToString(), "Preposto responsável:", "acima");
            Proposta.EmailPreposto = ExtrairValor(sb.ToString(), "E-mail:", "acima");
            Proposta.NumeroProposta = ExtrairValor(sb.ToString(), "NRO DA PROPOSTA TÉCNICA:", "acima");
            Proposta.NumeroPropostaComercial = ExtrairValor(sb.ToString(), "NRO DA PROPOSTA COMERCIAL:", "acima");
            Proposta.HorasTotais = decimal.TryParse(ExtrairValor(sb.ToString(), "TOTAL DE HORAS:", "lado"), out decimal horasTotais) ? horasTotais : 0;
            Proposta.LocalTrabalho = ExtrairValor(sb.ToString(), "LOCAL DE TRABALHO:", "acima");
            Proposta.Premissas = ExtrairBloco(sb.ToString(), "RESTRIÇÕES", "Nome");
            Proposta.DentroEscopo = ExtrairBloco(sb.ToString(), "DENTRO DO ESCOPO", "FORA DO ESCOPO");
            Proposta.ForaEscopo = ExtrairBloco(sb.ToString(), "FORA DO ESCOPO", "Nome");
            Proposta.DocumentoComplementar = ExtrairValor(sb.ToString(), "x", "acima");
            Proposta.DocumentoComplementar = ExtrairValor(sb.ToString(), "Existe documento complementar anexado nesta proposta?", "acima");

            string blocoPacotes = ExtrairBloco(sb.ToString(), "Arquitetura ", "TOTAL DE HORAS:");
            var linhas = blocoPacotes.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var registros = new List<PacoteEntity>();

            for (int i = 0; i < linhas.Length; i++)
            {
                try
                {
                    if (linhas[i].StartsWith("PACOTE"))
                    {
                        string pacoteLine = linhas[i];
                        string perfilLine = linhas[i + 2];

                        var match = Regex.Match(pacoteLine, @"PACOTE\s+(.*?)\s+QTD HORAS\s+(.*?)\s+DT INICIO\s+(.*?)\s+DT FIM\s+(.*)");

                        var registro = new PacoteEntity
                        {
                            IdPacote = Guid.NewGuid(),
                            IdProposta = Proposta.IdProposta,
                            Pacote = match.Success ? match.Groups[1].Value.Trim() : "",
                            Horas = match.Success ? match.Groups[2].Value.Trim() : "",
                            DataIni = match.Success ? match.Groups[3].Value.Trim() : "",
                            DataFim = match.Success ? match.Groups[4].Value.Trim() : "",
                            Perfil = perfilLine.Trim()
                        };

                        registros.Add(registro);
                    }
                }
                catch
                {
                }
            }
            Proposta.Pacotes = registros;

            string blocoTipoProposta = ExtrairBloco(sb.ToString(), "TIPO DE Proposta:", "PACOTE");
            StringBuilder tipoProposta = new(blocoTipoProposta.Replace("[", "").Replace("]", "|").Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("|", Environment.NewLine + "|"));

            string[] linhasP = tipoProposta.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < linhasP.Length; i++)
            {
                if (linhasP[i].Contains("X"))
                {
                    linhasP[i] = linhasP[i].Replace("X", "X" + (i + 1));
                }
            }
            tipoProposta.Clear();
            tipoProposta.Append(string.Join(Environment.NewLine, linhasP));

            if (tipoProposta.ToString().Contains("X2")) Proposta.TipoProposta.Analise = true;
            if (tipoProposta.ToString().Contains("X3")) Proposta.TipoProposta.Requisitos = true;
            if (tipoProposta.ToString().Contains("X4")) Proposta.TipoProposta.Testes = true;
            if (tipoProposta.ToString().Contains("X5")) Proposta.TipoProposta.Programacao = true;
            if (tipoProposta.ToString().Contains("X6")) Proposta.TipoProposta.AnaliseProgramacao = true;
            if (tipoProposta.ToString().Contains("X7")) Proposta.TipoProposta.Etl = true;
            if (tipoProposta.ToString().Contains("X8")) Proposta.TipoProposta.Arquitetura = true;
            if (tipoProposta.ToString().Contains("X9")) Proposta.TipoProposta.EspecificacaoExecucaoTestes = true;

            Proposta.Aceite = new StringBuilder(ExtrairBloco(sb.ToString(), "Termo de aceite:", "O uso deste modelo para ALOCAÇÃO É PROIBIDO"));

            return Proposta;
        }

        public async Task<int> InserirPropostaAsync(PropostaEntity Proposta)
        {
            if (Proposta is null)
            {
                throw new ArgumentNullException(nameof(Proposta));
            }

            _context.Transaction ??= _context.Connection.BeginTransaction();

            try
            {
                var idProjeto = Proposta.IdProposta;
                var idTipoProjeto = Proposta.TipoProposta.IdTipoProposta;
                var registrosAfetados = 0;

                registrosAfetados += await _context.Connection.ExecuteAsync(
                    InsertSqlProjeto,
                    new
                    {
                        IdProjeto = idProjeto,
                        Proposta.IdDocusign,
                        TipoProposta = Proposta.TipoPt,
                        Proposta.VersaoProposta,
                        Proposta.Vigencia,
                        Proposta.Fornecedor,
                        Proposta.Preposto,
                        Proposta.EmailPreposto,
                        Proposta.NumeroProposta,
                        Proposta.NumeroPropostaComercial,
                        Proposta.HorasTotais,
                        Proposta.LocalTrabalho,
                        Proposta.Premissas,
                        Proposta.DentroEscopo,
                        Proposta.ForaEscopo,
                        DocumentoComplementar = ParseDocumentoComplementar(Proposta.DocumentoComplementar),
                        Aceite = Proposta.Aceite.ToString()
                    },
                    _context.Transaction);

                registrosAfetados += await _context.Connection.ExecuteAsync(
                    InsertSqlTipoProjeto,
                    new
                    {
                        Id = idTipoProjeto,
                        Proposta.TipoProposta.Analise,
                        Proposta.TipoProposta.Requisitos,
                        Proposta.TipoProposta.AnaliseProgramacao,
                        Proposta.TipoProposta.Testes,
                        Proposta.TipoProposta.Programacao,
                        Proposta.TipoProposta.Etl,
                        Proposta.TipoProposta.Arquitetura,
                        Proposta.TipoProposta.EspecificacaoExecucaoTestes
                    },
                    _context.Transaction);

                registrosAfetados += await _context.Connection.ExecuteAsync(
                    InsertSqlProjetoTipoProjeto,
                    new
                    {
                        IdProjeto = idProjeto,
                        IdTipoProjeto = idTipoProjeto
                    },
                    _context.Transaction);

                foreach (var pacote in Proposta.Pacotes)
                {
                    registrosAfetados += await _context.Connection.ExecuteAsync(
                        InsertSqlPacote,
                        new
                        {
                            Id = pacote.IdPacote,
                            IdPacote = ParsePacoteId(pacote.Pacote),
                            Horas = ParseDecimal(pacote.Horas),
                            DataIni = ParseDate(pacote.DataIni),
                            DataFim = ParseDate(pacote.DataFim)
                        },
                        _context.Transaction);

                    registrosAfetados += await _context.Connection.ExecuteAsync(
                        InsertSqlProjetoPacote,
                        new
                        {
                            IdProjeto = idProjeto,
                            IdPacote = pacote.IdPacote
                        },
                        _context.Transaction);
                }

                _context.Transaction.Commit();
                _context.Transaction.Dispose();
                _context.Transaction = null;

                return registrosAfetados;
            }
            catch
            {
                _context.Transaction?.Rollback();
                _context.Transaction?.Dispose();
                _context.Transaction = null;
                throw;
            }
        }

        private static bool ParseDocumentoComplementar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return false;
            }

            var texto = valor.Trim().ToLowerInvariant();
            return texto is "1" or "true" or "sim" or "s" or "yes" or "y" || texto.Contains('x');
        }

        private static int? ParsePacoteId(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            var match = Regex.Match(valor, @"\d+");
            return match.Success && int.TryParse(match.Value, out var numero) ? numero : null;
        }

        private static decimal ParseDecimal(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return 0m;
            }

            if (decimal.TryParse(valor, NumberStyles.Any, new CultureInfo("pt-BR"), out var decimalPtBr))
            {
                return decimalPtBr;
            }

            if (decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalInvariant))
            {
                return decimalInvariant;
            }

            return 0m;
        }

        private static DateTime ParseDate(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return DateTime.MinValue.Date;
            }

            string[] formatos = ["dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd"];

            if (DateTime.TryParseExact(valor, formatos, new CultureInfo("pt-BR"), DateTimeStyles.None, out var data))
            {
                return data.Date;
            }

            if (DateTime.TryParse(valor, new CultureInfo("pt-BR"), DateTimeStyles.None, out data))
            {
                return data.Date;
            }

            return DateTime.MinValue.Date;
        }
    }
}
