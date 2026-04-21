# UtilitiesPdf em Python

Versao em Python da aplicacao atual, com backend e frontend no mesmo projeto.

## Stack

- `FastAPI` para API e paginas
- `Jinja2` para templates HTML
- `pypdf` para leitura de PDFs
- `openpyxl` para exportacao em Excel
- `sqlite` por padrao, com suporte opcional a SQL Server via `pyodbc`

## Como rodar

```powershell
cd C:\dev\UtilitiesPdf\python-app
python -m venv .venv
.venv\Scripts\Activate.ps1
pip install -r requirements.txt
Copy-Item .env.example .env
python -m uvicorn app.main:app --reload
```

Abra [http://127.0.0.1:8000](http://127.0.0.1:8000).

## O que foi portado

- Tela inicial
- Tela de importacoes
- Tela de follow-up com filtros e exportacao Excel
- Endpoints JSON equivalentes:
  - `GET /api/importacoes/historico`
  - `GET /api/importacoes/exportar`
  - `POST /api/propostas/importar`
  - `POST /api/propostas/importar-lote`
  - `POST /api/propostas/importar-pasta`

## Banco de dados

- Padrao: `sqlite` local em `utilities_pdf.db`
- Opcional: SQL Server, definindo `DATABASE_URL` no `.env`

Drivers suportados no codigo:

- `mssql+pypyodbc://...` para SQL Server usando o driver ODBC instalado no Windows
- `mssql+pytds://...` para SQL Server com driver puro-Python
- `mssql+pyodbc://...` quando houver ambiente compativel com `pyodbc`

Neste workspace, a rota mais estavel para SQL Server foi `mssql+pypyodbc://...`.

Quando o banco for SQL Server, a app tenta usar as mesmas tabelas da versao atual para proposta, tipo de proposta, pacote e historico.
