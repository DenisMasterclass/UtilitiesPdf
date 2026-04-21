from fastapi import APIRouter, Request
from fastapi.responses import HTMLResponse
from fastapi.templating import Jinja2Templates


router = APIRouter()
templates = Jinja2Templates(directory="app/templates")


@router.get("/", response_class=HTMLResponse)
def home(request: Request) -> HTMLResponse:
    return templates.TemplateResponse("home.html", {"request": request, "active_page": "home"})


@router.get("/importacoes", response_class=HTMLResponse)
def importacoes(request: Request) -> HTMLResponse:
    return templates.TemplateResponse("importacoes.html", {"request": request, "active_page": "importacoes"})


@router.get("/follow-up", response_class=HTMLResponse)
def follow_up(request: Request) -> HTMLResponse:
    return templates.TemplateResponse("followup.html", {"request": request, "active_page": "followup"})
