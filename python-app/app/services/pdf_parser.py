from __future__ import annotations

import re
from pathlib import Path

from pypdf import PdfReader

from app.models.dto import Pacote, Proposta
from app.models.types import TipoPt


class PdfParserService:
    @staticmethod
    def pdf_to_text(file_path: Path) -> str:
        reader = PdfReader(str(file_path))
        pages = [page.extract_text() or "" for page in reader.pages]
        return "\n".join(pages)

    def parse_proposta(self, text: str, tipo_pt: TipoPt) -> Proposta:
        proposta = Proposta(tipo_pt=tipo_pt)
        proposta.id_docusign = self.extract_value(text, "Docusign Envelope ID:", "lado")
        proposta.versao_proposta = self.extract_value(text, "Vigencia Atualizacao Publicacao Versao", "abaixo")
        proposta.vigencia = self.extract_block(
            text,
            "GESTAO_FORNEC_103" if tipo_pt == TipoPt.PROJETO else "GESTAO_FORNEC_101",
            "GESTAO DE FORNECEDORES DE TI",
        )
        proposta.fornecedor = self.extract_value(text, "FORNECEDOR:", "abaixo" if tipo_pt == TipoPt.PROJETO else "lado")
        proposta.preposto = self.extract_value(text, "Preposto responsavel:", "acima" if tipo_pt == TipoPt.PROJETO else "abaixo")
        proposta.email_preposto = self.extract_value(text, "E-mail:", "acima")
        proposta.numero_proposta = self.extract_value(text, "NRO DA PROPOSTA TECNICA:", "acima")
        proposta.numero_proposta_comercial = self.extract_value(text, "NRO DA PROPOSTA COMERCIAL:", "acima")
        proposta.local_trabalho = self.extract_value(text, "LOCAL DE TRABALHO:", "acima")
        proposta.premissas = self.extract_block(text, "RESTRICOES", "Nome")
        proposta.dentro_escopo = self.extract_block(text, "DENTRO DO ESCOPO", "FORA DO ESCOPO")
        proposta.fora_escopo = self.extract_block(text, "FORA DO ESCOPO", "Nome")
        proposta.documento_complementar = self.extract_value(
            text,
            "Existe documento complementar anexado nesta proposta?",
            "acima",
        )
        proposta.aceite = self.extract_block(text, "Termo de aceite:", "O uso deste modelo para ALOCACAO E PROIBIDO")

        horas_texto = self.extract_value(text, "TOTAL DE HORAS:", "lado")
        proposta.horas_totais = self.parse_float(horas_texto)
        if tipo_pt == TipoPt.ALOCACAO:
            proposta.gestor_contratante = self.extract_value(text, "Nome do Gestor Porto Contratante:", "abaixo")

        proposta.pacotes = self.extract_pacotes(text, proposta.id_proposta)
        proposta.tipo_proposta = self.extract_tipo_proposta(text, proposta.id_proposta)
        return proposta

    def extract_value(self, text: str, marker: str, position: str = "abaixo") -> str:
        normalized_text = self.normalize(text)
        normalized_marker = self.normalize(marker)
        lines = normalized_text.splitlines()
        for index, line in enumerate(lines):
            if normalized_marker in line:
                if position == "abaixo":
                    return lines[index + 1].strip() if index + 1 < len(lines) else ""
                if position == "acima":
                    return lines[index - 1].strip() if index > 0 else ""
                if position == "lado":
                    return line.replace(normalized_marker, "").strip()
        return ""

    def extract_block(self, text: str, start_marker: str, end_marker: str) -> str:
        normalized = self.normalize(text)
        start = self.normalize(start_marker)
        end = self.normalize(end_marker)
        match = re.search(f"{re.escape(start)}(.*?){re.escape(end)}", normalized, flags=re.DOTALL)
        return match.group(1).strip() if match else ""

    def extract_pacotes(self, text: str, id_proposta) -> list[Pacote]:
        block = self.extract_block(text, "Arquitetura", "TOTAL DE HORAS:")
        lines = [line.strip() for line in block.splitlines() if line.strip()]
        pacotes: list[Pacote] = []
        for index, line in enumerate(lines):
            if not line.startswith("PACOTE"):
                continue
            perfil = lines[index + 2] if index + 2 < len(lines) else ""
            match = re.search(r"PACOTE\s+(.*?)\s+QTD HORAS\s+(.*?)\s+DT INICIO\s+(.*?)\s+DT FIM\s+(.*)", line)
            pacotes.append(
                Pacote(
                    id_proposta=id_proposta,
                    pacote=match.group(1).strip() if match else "",
                    horas=match.group(2).strip() if match else "",
                    data_ini=match.group(3).strip() if match else "",
                    data_fim=match.group(4).strip() if match else "",
                    perfil=perfil,
                )
            )
        return pacotes

    def extract_tipo_proposta(self, text: str, id_proposta):
        from app.models.dto import TipoPropostaFlags

        block = self.extract_block(text, "TIPO DE Proposta:", "PACOTE")
        normalized = block.replace("[", "").replace("]", "|").replace(" ", "")
        normalized = normalized.replace("\r\n", "").replace("\n", "").replace("|", "\n|")
        lines = normalized.splitlines()
        marks = []
        for index, line in enumerate(lines):
            if "X" in line.upper():
                marks.append(index + 1)

        return TipoPropostaFlags(
            analise=2 in marks,
            requisitos=3 in marks,
            testes=4 in marks,
            programacao=5 in marks,
            analise_programacao=6 in marks,
            etl=7 in marks,
            arquitetura=8 in marks,
            especificacao_execucao_testes=9 in marks,
        )

    @staticmethod
    def parse_float(value: str) -> float:
        if not value:
            return 0
        normalized = value.replace(".", "").replace(",", ".")
        try:
            return float(normalized)
        except ValueError:
            return 0

    @staticmethod
    def normalize(value: str) -> str:
        replacements = {
            "Á": "A",
            "À": "A",
            "Ã": "A",
            "Â": "A",
            "É": "E",
            "Ê": "E",
            "Í": "I",
            "Ó": "O",
            "Ô": "O",
            "Õ": "O",
            "Ú": "U",
            "Ç": "C",
            "á": "a",
            "à": "a",
            "ã": "a",
            "â": "a",
            "é": "e",
            "ê": "e",
            "í": "i",
            "ó": "o",
            "ô": "o",
            "õ": "o",
            "ú": "u",
            "ç": "c",
        }
        result = value
        for source, target in replacements.items():
            result = result.replace(source, target)
        return result
