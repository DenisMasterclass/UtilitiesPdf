from __future__ import annotations

from io import BytesIO

from openpyxl import Workbook

from app.models.dto import ImportacaoHistoricoItem


class ExcelService:
    @staticmethod
    def gerar_followup(items: list[ImportacaoHistoricoItem]) -> bytes:
        workbook = Workbook()
        worksheet = workbook.active
        worksheet.title = "FollowUp"
        headers = ["Data", "Arquivo", "Origem", "Status", "Registros", "Mensagem"]
        worksheet.append(headers)

        for item in items:
            worksheet.append(
                [
                    item.data_processamento,
                    item.nome_arquivo,
                    item.origem,
                    "Sucesso" if item.sucesso else "Falhou",
                    item.registros_afetados,
                    item.mensagem,
                ]
            )

        worksheet.column_dimensions["A"].width = 22
        worksheet.column_dimensions["B"].width = 36
        worksheet.column_dimensions["C"].width = 16
        worksheet.column_dimensions["D"].width = 14
        worksheet.column_dimensions["E"].width = 12
        worksheet.column_dimensions["F"].width = 64

        for row in worksheet.iter_rows(min_row=2, max_col=1):
            for cell in row:
                cell.number_format = "DD/MM/YYYY HH:MM"

        stream = BytesIO()
        workbook.save(stream)
        return stream.getvalue()
