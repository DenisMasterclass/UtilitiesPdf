using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Utils.Repositories.Entities;
using Utils.Shared;

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


        public async Task<ProjetoEntity> CamposProjetos(StringBuilder sb)
        {
            ProjetoEntity projeto = new ProjetoEntity();
            PacoteEntity pacote = new PacoteEntity();

            projeto.IdProjeto = Guid.NewGuid();
            projeto.IdDocusign = ExtrairValor(sb.ToString(), "Docusign Envelope ID:", "lado");
            projeto.TipoProposta = ExtrairValor(sb.ToString(), "Identificador", "abaixo");
            projeto.VersaoProposta = ExtrairValor(sb.ToString(), "Vigência Atualização Publicação Versão", "abaixo");
            projeto.Vigencia = ExtrairBloco(sb.ToString(), "Vigência Atualização Publicação Versão", "GESTÃO DE FORNECEDORES DE TI");
            projeto.Fornecedor = ExtrairValor(sb.ToString(), "FORNECEDOR:", "abaixo");
            projeto.Preposto = ExtrairValor(sb.ToString(), "Preposto responsável:", "acima");
            projeto.EmailPreposto = ExtrairValor(sb.ToString(), "E-mail:", "acima");
            projeto.NumeroProposta = ExtrairValor(sb.ToString(), "NRO DA PROPOSTA TÉCNICA:", "acima");
            projeto.NumeroPropostaComercial = ExtrairValor(sb.ToString(), "NRO DA PROPOSTA COMERCIAL:", "acima");
            projeto.HorasTotais = decimal.TryParse(ExtrairValor(sb.ToString(), "TOTAL DE HORAS:", "lado"), out decimal horasTotais) ? horasTotais : 0;
            projeto.LocalTrabalho = ExtrairValor(sb.ToString(), "LOCAL DE TRABALHO:", "acima");
            projeto.Premissas = ExtrairBloco(sb.ToString(), "RESTRIÇÕES", "Nome");
            projeto.DentroEscopo = ExtrairBloco(sb.ToString(), "DENTRO DO ESCOPO", "FORA DO ESCOPO");
            projeto.ForaEscopo = ExtrairBloco(sb.ToString(), "FORA DO ESCOPO", "Nome");
            projeto.DocumentoComplementar = ExtrairValor(sb.ToString(), "x", "acima");
            projeto.DocumentoComplementar = ExtrairValor(sb.ToString(), "Existe documento complementar anexado nesta proposta?", "acima");
            //pacote
            string BlocoPacotex = ExtrairBloco(sb.ToString(), "Arquitetura ", "TOTAL DE HORAS:");
            var linhas = BlocoPacotex.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var registros = new List<PacoteEntity>();

            for (int i = 0; i < linhas.Length; i++)
            {
                if (linhas[i].StartsWith("PACOTE"))
                {
                    // Linha do PACOTE pode ter os valores misturados
                    string pacoteLine = linhas[i];
                    string perfilLine = linhas[i + 2]; // pula "*PERFIL" e pega a linha seguinte

                    // Regex para capturar os valores (exemplo simplificado)
                    var match = Regex.Match(pacoteLine, @"PACOTE\s+(.*?)\s+QTD HORAS\s+(.*?)\s+DT INICIO\s+(.*?)\s+DT FIM\s+(.*)");

                    var registro = new PacoteEntity
                    {
                        Pacote = match.Success ? match.Groups[1].Value.Trim() : "",
                        Horas = match.Success ? match.Groups[2].Value.Trim() : "",
                        DataIni = match.Success ? match.Groups[3].Value.Trim() : "",
                        DataFim = match.Success ? match.Groups[4].Value.Trim() : "",
                        Perfil = perfilLine.Trim()
                    };

                    registros.Add(registro);
                }
            }
            projeto.Pacotes = registros;

            //tipo projeto
            string BlocoTipoProjetox = ExtrairBloco(sb.ToString(), "TIPO DE PROJETO:", "PACOTE");
            StringBuilder TipoProjetox = new(BlocoTipoProjetox.Replace("[", "").Replace("]", "|").Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("|", Environment.NewLine + "|"));

            string[] linhasP = TipoProjetox.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < linhasP.Length; i++)
            {
                if (linhasP[i].Contains("X"))
                {
                    // Substitui X pelo número da linha (i+1 porque índice começa em 0)
                    linhasP[i] = linhasP[i].Replace("X", "X" + (i + 1));
                }
            }
            TipoProjetox.Clear();
            TipoProjetox.Append(string.Join(Environment.NewLine, linhas));

            if (TipoProjetox.ToString().Contains("X2")) projeto.TipoProjeto.Analise = true;
            if (TipoProjetox.ToString().Contains("X3")) projeto.TipoProjeto.Requisitos = true;
            if (TipoProjetox.ToString().Contains("X4")) projeto.TipoProjeto.Testes = true;
            if (TipoProjetox.ToString().Contains("X5")) projeto.TipoProjeto.Programacao = true;
            if (TipoProjetox.ToString().Contains("X6")) projeto.TipoProjeto.AnaliseProgramacao = true;
            if (TipoProjetox.ToString().Contains("X7")) projeto.TipoProjeto.Etl = true;
            if (TipoProjetox.ToString().Contains("X8")) projeto.TipoProjeto.Arquitetura = true;
            if (TipoProjetox.ToString().Contains("X9")) projeto.TipoProjeto.EspecificacaoExecucaoTestes = true;

            projeto.Aceite = new StringBuilder(ExtrairBloco(sb.ToString(), "Termo de aceite:", "O uso deste modelo para ALOCAÇÃO É PROIBIDO"));


            return projeto;
        }

    }
}
