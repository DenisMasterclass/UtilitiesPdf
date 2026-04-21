from datetime import date, datetime, time

from fastapi import APIRouter, Query
from fastapi.responses import StreamingResponse

from app.db.repository import DatabaseRepository
from app.services.excel_service import ExcelService


router = APIRouter(prefix="/api/importacoes", tags=["importacoes"])
repository = DatabaseRepository.instance()


@router.get("/historico")
def listar_historico(
    data_inicial: date | None = Query(default=None, alias="dataInicial"),
    data_final: date | None = Query(default=None, alias="dataFinal"),
):
    data_inicial_dt = datetime.combine(data_inicial, time.min) if data_inicial else None
    data_final_dt = datetime.combine(data_final, time.min) if data_final else None
    return repository.list_history(data_inicial_dt, data_final_dt)


@router.get("/exportar")
def exportar_historico(
    data_inicial: date | None = Query(default=None, alias="dataInicial"),
    data_final: date | None = Query(default=None, alias="dataFinal"),
):
    data_inicial_dt = datetime.combine(data_inicial, time.min) if data_inicial else None
    data_final_dt = datetime.combine(data_final, time.min) if data_final else None
    items = repository.list_history(data_inicial_dt, data_final_dt)
    content = ExcelService.gerar_followup(items)
    filename = "followup-analitico.xlsx"
    headers = {"Content-Disposition": f'attachment; filename="{filename}"'}
    return StreamingResponse(
        iter([content]),
        media_type="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        headers=headers,
    )
