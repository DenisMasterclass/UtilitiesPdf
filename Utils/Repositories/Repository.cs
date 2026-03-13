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
            TipoProjetoEntity tipoProjeto = new TipoProjetoEntity();

            projeto.IdProjeto = Guid.NewGuid();
            projeto.IdDocusign = ExtrairValor(sb.ToString(), "Docusign Envelope ID:", "lado");
            projeto.TipoProposta = ExtrairValor(sb.ToString(), "Identificador", "abaixo");
            projeto.VersaoProposta = ExtrairValor(sb.ToString(), "Vigência Atualização Publicação Versão", "abaixo");
            projeto.Vigencia = ExtrairValor(sb.ToString(), "Vigência", "abaixo");
            projeto.Fornecedor = ExtrairValor(sb.ToString(), "FORNECEDOR:", "abaixo");
            projeto.Preposto = ExtrairValor(sb.ToString(), "Preposto responsável:", "acima");
            projeto.EmailPreposto = ExtrairValor(sb.ToString(), "E-mail:", "acima");
            projeto.NumeroProposta = ExtrairValor(sb.ToString(), "NRO DA PROPOSTA TÉCNICA:", "acima");
            projeto.NumeroPropostaComercial = ExtrairValor(sb.ToString(), "NRO DA PROPOSTA COMERCIAL:", "acima");
            projeto.HorasTotais = decimal.TryParse(ExtrairValor(sb.ToString(), "TOTAL DE HORAS:", "lado"), out decimal horasTotais) ? horasTotais : 0;
            projeto.LocalTrabalho = ExtrairValor(sb.ToString(), "LOCAL DE TRABALHO:", "acima");
            projeto.Premissas = ExtrairBloco(sb.ToString(), "PREMISSAS E RESTRIÇÕES", "Nome do Arquivo PT - Proposta Técnica - PROJETO Página 2 de 4");
            projeto.DentroEscopo = ExtrairBloco(sb.ToString(), "DENTRO DO ESCOPO", "FORA DO ESCOPO");
            projeto.ForaEscopo = ExtrairBloco(sb.ToString(), "FORA DO ESCOPO", "Nome do Arquivo PT - Proposta Técnica - PROJETO Página 3 de 4");
            projeto.DocumentoComplementar = ExtrairValor(sb.ToString(), "Existe documento complementar anexado nesta proposta?", "acima");
            projeto.Aceite = new StringBuilder(ExtrairBloco(sb.ToString(), "Termo de aceite:", "O uso deste modelo para ALOCAÇÃO É PROIBIDO"));

            // Serializa para JSON com indentação
            string json = JsonSerializer.Serialize(projeto, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            Console.WriteLine(json);

            return projeto;
        }
        public async Task<ProjetoEntity> ReadProject(ProjetoEntity projetoEntity)
        {
            var ListTipoProjeto = new List<TipoProjetoEntity>();
            var ListPacote = new List<PacoteEntity>();

            var ListProjeto = new ProjetoEntity
            {
                IdProjeto = projetoEntity.IdProjeto,
                IdDocusign = projetoEntity.IdDocusign,
                TipoProposta = projetoEntity.TipoProposta,
                VersaoProposta = projetoEntity.VersaoProposta,
                Vigencia = projetoEntity.Vigencia,
                Fornecedor = projetoEntity.Fornecedor,
                Preposto = projetoEntity.Preposto,
                EmailPreposto = projetoEntity.EmailPreposto,
                NumeroProposta = projetoEntity.NumeroProposta,
                NumeroPropostaComercial = projetoEntity.NumeroPropostaComercial,
                TipoProjeto = projetoEntity.TipoProjeto,
                Pacotes = projetoEntity.Pacotes,
                HorasTotais = projetoEntity.HorasTotais,
                LocalTrabalho = projetoEntity.LocalTrabalho,
                Premissas = projetoEntity.Premissas,
                DentroEscopo = projetoEntity.DentroEscopo,
                ForaEscopo = projetoEntity.ForaEscopo,
                DocumentoComplementar = projetoEntity.DocumentoComplementar,
                Aceite = projetoEntity.Aceite
            };


            return ListProjeto;
        }
    }
}
