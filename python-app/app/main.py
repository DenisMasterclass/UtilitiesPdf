from fastapi import FastAPI
from fastapi.staticfiles import StaticFiles

from app.api.pages import router as pages_router
from app.api.propostas import router as propostas_router
from app.api.importacoes import router as importacoes_router
from app.core.config import get_settings
from app.db.repository import DatabaseRepository


settings = get_settings()
app = FastAPI(title=settings.app_title, debug=settings.app_debug)
app.mount("/static", StaticFiles(directory="app/static"), name="static")
app.include_router(pages_router)
app.include_router(propostas_router)
app.include_router(importacoes_router)


@app.on_event("startup")
def startup() -> None:
    DatabaseRepository.instance().initialize()
