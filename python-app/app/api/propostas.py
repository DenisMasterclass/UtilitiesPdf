from typing import Annotated

from fastapi import APIRouter, File, Query, UploadFile
from fastapi.responses import JSONResponse
from pydantic import AliasChoices, BaseModel, Field

from app.models.types import TipoPt
from app.services.import_service import ImportService


class ImportarPastaPayload(BaseModel):
    caminho_pasta: str = Field(validation_alias=AliasChoices("caminho_pasta", "caminhoPasta"))


router = APIRouter(prefix="/api/propostas", tags=["propostas"])
service = ImportService()


@router.post("/importar")
async def importar_pdf(
    arquivo: Annotated[UploadFile, File(...)],
    tipo_pt: TipoPt = Query(..., alias="tipoPt"),
):
    resultado = await service.processar_arquivo_upload(arquivo, tipo_pt)
    if resultado.sucesso:
        return resultado
    return JSONResponse(status_code=400, content=resultado.model_dump(mode="json"))


@router.post("/importar-lote")
async def importar_pdfs(
    arquivos: Annotated[list[UploadFile], File(...)],
    tipo_pt: TipoPt = Query(..., alias="tipoPt"),
):
    return await service.processar_arquivos_upload(arquivos, tipo_pt)


@router.post("/importar-pasta")
async def importar_pasta(
    payload: ImportarPastaPayload,
    tipo_pt: TipoPt = Query(..., alias="tipoPt"),
):
    return service.processar_pasta(payload.caminho_pasta, tipo_pt)
