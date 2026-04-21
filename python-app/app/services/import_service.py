from __future__ import annotations

import tempfile
from datetime import datetime
from pathlib import Path

from fastapi import UploadFile

from app.db.repository import DatabaseRepository
from app.models.dto import ImportacaoHistoricoItem, ImportacaoPdfLoteResultado, ImportacaoPdfResultado
from app.models.types import TipoPt
from app.services.pdf_parser import PdfParserService


class ImportService:
    def __init__(self) -> None:
        self.parser = PdfParserService()
        self.repository = DatabaseRepository.instance()

    async def processar_arquivo_upload(self, arquivo: UploadFile, tipo_pt: TipoPt) -> ImportacaoPdfResultado:
        if arquivo is None or not arquivo.filename:
            return self._base_resultado(
                ImportacaoPdfResultado(
                    nome_arquivo="",
                    sucesso=False,
                    mensagem="Arquivo PDF nao informado ou vazio.",
                ),
                tipo_pt,
            )

        suffix = Path(arquivo.filename).suffix or ".pdf"
        with tempfile.NamedTemporaryFile(delete=False, suffix=suffix) as temp_file:
            temp_file.write(await arquivo.read())
            temp_path = Path(temp_file.name)

        try:
            return self.processar_arquivo_disco(temp_path, arquivo.filename, "Upload", tipo_pt)
        finally:
            temp_path.unlink(missing_ok=True)

    async def processar_arquivos_upload(self, arquivos: list[UploadFile], tipo_pt: TipoPt) -> ImportacaoPdfLoteResultado:
        resultados: list[ImportacaoPdfResultado] = []
        for arquivo in arquivos:
            resultados.append(await self.processar_arquivo_upload(arquivo, tipo_pt))
        return self._resultado_lote(resultados, tipo_pt)

    def processar_pasta(self, caminho_pasta: str, tipo_pt: TipoPt) -> ImportacaoPdfLoteResultado:
        if not caminho_pasta.strip():
            return self._resultado_lote(
                [self._base_resultado(ImportacaoPdfResultado(nome_arquivo="", sucesso=False, mensagem="Caminho da pasta nao informado."), tipo_pt)],
                tipo_pt,
            )

        pasta = Path(caminho_pasta)
        if not pasta.exists() or not pasta.is_dir():
            return self._resultado_lote(
                [self._base_resultado(ImportacaoPdfResultado(nome_arquivo=caminho_pasta, sucesso=False, mensagem="Pasta nao encontrada."), tipo_pt)],
                tipo_pt,
            )

        resultados = [
            self.processar_arquivo_disco(arquivo, arquivo.name, "Pasta", tipo_pt)
            for arquivo in sorted(pasta.glob("*.pdf"))
        ]
        return self._resultado_lote(resultados, tipo_pt)

    def processar_arquivo_disco(self, caminho_arquivo: Path, nome_arquivo: str, origem: str, tipo_pt: TipoPt) -> ImportacaoPdfResultado:
        try:
            texto = self.parser.pdf_to_text(caminho_arquivo)
            proposta = self.parser.parse_proposta(texto, tipo_pt)
            registros_afetados = self.repository.insert_proposta(proposta)
            resultado = ImportacaoPdfResultado(
                nome_arquivo=nome_arquivo,
                sucesso=True,
                registros_afetados=registros_afetados,
                mensagem="Arquivo processado com sucesso.",
            )
        except Exception as exc:
            resultado = ImportacaoPdfResultado(
                nome_arquivo=nome_arquivo,
                sucesso=False,
                registros_afetados=0,
                mensagem=str(exc),
            )

        self.repository.save_history(
            ImportacaoHistoricoItem(
                nome_arquivo=resultado.nome_arquivo,
                origem=origem,
                sucesso=resultado.sucesso,
                registros_afetados=resultado.registros_afetados,
                mensagem=resultado.mensagem or "",
                data_processamento=datetime.now(),
            )
        )
        return self._base_resultado(resultado, tipo_pt)

    @staticmethod
    def _base_resultado(resultado: ImportacaoPdfResultado, tipo_pt: TipoPt) -> ImportacaoPdfResultado:
        resultado.tipo_pt = tipo_pt
        resultado.tipo_pt_label = tipo_pt.label
        return resultado

    @staticmethod
    def _resultado_lote(resultados: list[ImportacaoPdfResultado], tipo_pt: TipoPt) -> ImportacaoPdfLoteResultado:
        return ImportacaoPdfLoteResultado(
            total_arquivos=len(resultados),
            arquivos_processados_com_sucesso=sum(1 for item in resultados if item.sucesso),
            arquivos_com_erro=sum(1 for item in resultados if not item.sucesso),
            tipo_pt=tipo_pt,
            tipo_pt_label=tipo_pt.label,
            resultados=resultados,
        )
