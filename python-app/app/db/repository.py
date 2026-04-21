from __future__ import annotations

import sqlite3
from contextlib import closing
from datetime import datetime, timedelta
from pathlib import Path
from threading import Lock
from urllib.parse import parse_qs, unquote, urlparse

try:
    import pyodbc  # type: ignore
except ImportError:  # pragma: no cover
    pyodbc = None

try:
    import pypyodbc  # type: ignore
except ImportError:  # pragma: no cover
    pypyodbc = None

try:
    import pytds  # type: ignore
except ImportError:  # pragma: no cover
    pytds = None

try:
    import certifi  # type: ignore
except ImportError:  # pragma: no cover
    certifi = None

from app.core.config import get_settings
from app.models.dto import ImportacaoHistoricoItem, Proposta


class DatabaseRepository:
    _instance: "DatabaseRepository | None" = None
    _lock = Lock()

    def __init__(self) -> None:
        self.settings = get_settings()
        self.database_url = self.settings.database_url
        self.is_sqlite = self.database_url.startswith("sqlite:///")
        self.is_pytds = self.database_url.startswith("mssql+pytds://")
        self.is_pypyodbc = self.database_url.startswith("mssql+pypyodbc://")
        self._sqlite_path = ""
        self._odbc_connection_string = ""
        self._pypyodbc_connection_string = ""
        self._pytds_config: dict[str, object] = {}

        if self.is_sqlite:
            self._sqlite_path = self.database_url.replace("sqlite:///", "", 1)
        elif self.is_pypyodbc:
            if pypyodbc is None:
                raise ValueError("pypyodbc nao esta instalado. Use sqlite:/// ou instale pypyodbc.")
            self._pypyodbc_connection_string = self._build_odbc_connection_string(self.database_url)
        elif self.is_pytds:
            if pytds is None:
                raise ValueError("python-tds nao esta instalado. Use sqlite:/// ou instale python-tds.")
            self._pytds_config = self._build_pytds_config(self.database_url)
        elif self.database_url.startswith("mssql+pyodbc://"):
            if pyodbc is None:
                raise ValueError("pyodbc nao esta instalado. Use sqlite:/// ou instale pyodbc em uma versao compativel do Python.")
            self._odbc_connection_string = self._build_odbc_connection_string(self.database_url)
        else:
            raise ValueError("DATABASE_URL precisa usar sqlite:///, mssql+pyodbc:// ou mssql+pytds://")

    @classmethod
    def instance(cls) -> "DatabaseRepository":
        if cls._instance is None:
            with cls._lock:
                if cls._instance is None:
                    cls._instance = cls()
        return cls._instance

    def initialize(self) -> None:
        if self.is_sqlite:
            self._initialize_sqlite()
            return
        if self.is_pypyodbc:
            self._initialize_sql_server_pypyodbc()
            return
        if self.is_pytds:
            self._initialize_sql_server_pytds()
            return
        self._initialize_sql_server()

    def insert_proposta(self, proposta: Proposta) -> int:
        if self.is_sqlite:
            return self._insert_proposta_sqlite(proposta)
        if self.is_pypyodbc:
            return self._insert_proposta_sql_server_pypyodbc(proposta)
        if self.is_pytds:
            return self._insert_proposta_sql_server_pytds(proposta)
        return self._insert_proposta_sql_server(proposta)

    def save_history(self, item: ImportacaoHistoricoItem) -> None:
        if self.is_sqlite:
            self._save_history_sqlite(item)
            return
        if self.is_pypyodbc:
            self._save_history_sql_server_pypyodbc(item)
            return
        if self.is_pytds:
            self._save_history_sql_server_pytds(item)
            return
        self._save_history_sql_server(item)

    def list_history(self, data_inicial: datetime | None, data_final: datetime | None) -> list[ImportacaoHistoricoItem]:
        if self.is_sqlite:
            return self._list_history_sqlite(data_inicial, data_final)
        if self.is_pypyodbc:
            return self._list_history_sql_server_pypyodbc(data_inicial, data_final)
        if self.is_pytds:
            return self._list_history_sql_server_pytds(data_inicial, data_final)
        return self._list_history_sql_server(data_inicial, data_final)

    def _sqlite_connection(self) -> sqlite3.Connection:
        Path(self._sqlite_path).parent.mkdir(parents=True, exist_ok=True)
        connection = sqlite3.connect(self._sqlite_path)
        connection.row_factory = sqlite3.Row
        return connection

    def _initialize_sqlite(self) -> None:
        with closing(self._sqlite_connection()) as connection:
            cursor = connection.cursor()
            cursor.executescript(
                """
                CREATE TABLE IF NOT EXISTS proposta (
                    id_proposta TEXT PRIMARY KEY,
                    id_docusign TEXT,
                    tipo_proposta INTEGER NOT NULL,
                    versao_proposta TEXT,
                    vigencia TEXT,
                    fornecedor TEXT,
                    preposto TEXT,
                    email_preposto TEXT,
                    numero_proposta TEXT,
                    numero_proposta_comercial TEXT,
                    gestor_contratante TEXT,
                    horas_totais REAL NOT NULL DEFAULT 0,
                    local_trabalho TEXT,
                    premissas TEXT,
                    dentro_escopo TEXT,
                    fora_escopo TEXT,
                    documento_complementar INTEGER NOT NULL DEFAULT 0,
                    aceite TEXT
                );

                CREATE TABLE IF NOT EXISTS tipo_proposta (
                    id_tipo_proposta TEXT PRIMARY KEY,
                    analise INTEGER NOT NULL,
                    requisitos INTEGER NOT NULL,
                    analise_programacao INTEGER NOT NULL,
                    testes INTEGER NOT NULL,
                    programacao INTEGER NOT NULL,
                    etl INTEGER NOT NULL,
                    arquitetura INTEGER NOT NULL,
                    especificacao_execucao_testes INTEGER NOT NULL,
                    id_proposta TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS pacote (
                    id_pacote TEXT PRIMARY KEY,
                    id_proposta TEXT NOT NULL,
                    pacote TEXT,
                    horas TEXT,
                    data_ini TEXT,
                    data_fim TEXT,
                    perfil TEXT
                );

                CREATE TABLE IF NOT EXISTS importacao_arquivo_historico (
                    id TEXT PRIMARY KEY,
                    nome_arquivo TEXT NOT NULL,
                    origem TEXT NOT NULL,
                    sucesso INTEGER NOT NULL,
                    registros_afetados INTEGER NOT NULL DEFAULT 0,
                    mensagem TEXT NOT NULL,
                    data_processamento TEXT NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_importacao_historico_data
                    ON importacao_arquivo_historico (data_processamento DESC);
                """
            )
            connection.commit()

    def _initialize_sql_server(self) -> None:
        commands = [
            """
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ImportacaoArquivoHistorico' AND schema_id = SCHEMA_ID('dbo'))
            BEGIN
                CREATE TABLE dbo.ImportacaoArquivoHistorico
                (
                    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ImportacaoArquivoHistorico PRIMARY KEY,
                    NomeArquivo NVARCHAR(260) NOT NULL,
                    Origem NVARCHAR(50) NOT NULL,
                    Sucesso BIT NOT NULL,
                    RegistrosAfetados INT NOT NULL CONSTRAINT DF_ImportacaoArquivoHistorico_RegistrosAfetados DEFAULT ((0)),
                    Mensagem NVARCHAR(MAX) NOT NULL,
                    DataProcessamento DATETIME2 NOT NULL CONSTRAINT DF_ImportacaoArquivoHistorico_DataProcessamento DEFAULT (SYSUTCDATETIME())
                );

                CREATE INDEX IX_ImportacaoArquivoHistorico_DataProcessamento
                    ON dbo.ImportacaoArquivoHistorico (DataProcessamento DESC);
            END
            """
        ]

        with closing(pyodbc.connect(self._odbc_connection_string, autocommit=True)) as connection:
            cursor = connection.cursor()
            for command in commands:
                cursor.execute(command)

    def _initialize_sql_server_pypyodbc(self) -> None:
        commands = [
            """
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ImportacaoArquivoHistorico' AND schema_id = SCHEMA_ID('dbo'))
            BEGIN
                CREATE TABLE dbo.ImportacaoArquivoHistorico
                (
                    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ImportacaoArquivoHistorico PRIMARY KEY,
                    NomeArquivo NVARCHAR(260) NOT NULL,
                    Origem NVARCHAR(50) NOT NULL,
                    Sucesso BIT NOT NULL,
                    RegistrosAfetados INT NOT NULL CONSTRAINT DF_ImportacaoArquivoHistorico_RegistrosAfetados DEFAULT ((0)),
                    Mensagem NVARCHAR(MAX) NOT NULL,
                    DataProcessamento DATETIME2 NOT NULL CONSTRAINT DF_ImportacaoArquivoHistorico_DataProcessamento DEFAULT (SYSUTCDATETIME())
                );

                CREATE INDEX IX_ImportacaoArquivoHistorico_DataProcessamento
                    ON dbo.ImportacaoArquivoHistorico (DataProcessamento DESC);
            END
            """
        ]
        connection = self._connect_pypyodbc(autocommit=True)
        try:
            cursor = connection.cursor()
            for command in commands:
                cursor.execute(command)
        finally:
            self._safe_close_pypyodbc(connection)

    def _initialize_sql_server_pytds(self) -> None:
        commands = [
            """
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ImportacaoArquivoHistorico' AND schema_id = SCHEMA_ID('dbo'))
            BEGIN
                CREATE TABLE dbo.ImportacaoArquivoHistorico
                (
                    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ImportacaoArquivoHistorico PRIMARY KEY,
                    NomeArquivo NVARCHAR(260) NOT NULL,
                    Origem NVARCHAR(50) NOT NULL,
                    Sucesso BIT NOT NULL,
                    RegistrosAfetados INT NOT NULL CONSTRAINT DF_ImportacaoArquivoHistorico_RegistrosAfetados DEFAULT ((0)),
                    Mensagem NVARCHAR(MAX) NOT NULL,
                    DataProcessamento DATETIME2 NOT NULL CONSTRAINT DF_ImportacaoArquivoHistorico_DataProcessamento DEFAULT (SYSUTCDATETIME())
                );

                CREATE INDEX IX_ImportacaoArquivoHistorico_DataProcessamento
                    ON dbo.ImportacaoArquivoHistorico (DataProcessamento DESC);
            END
            """
        ]
        with closing(pytds.connect(**self._pytds_config, autocommit=True)) as connection:
            cursor = connection.cursor()
            for command in commands:
                cursor.execute(command)

    def _insert_proposta_sqlite(self, proposta: Proposta) -> int:
        with closing(self._sqlite_connection()) as connection:
            cursor = connection.cursor()
            cursor.execute(
                """
                INSERT INTO proposta (
                    id_proposta, id_docusign, tipo_proposta, versao_proposta, vigencia, fornecedor, preposto,
                    email_preposto, numero_proposta, numero_proposta_comercial, gestor_contratante, horas_totais,
                    local_trabalho, premissas, dentro_escopo, fora_escopo, documento_complementar, aceite
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                """,
                (
                    str(proposta.id_proposta),
                    proposta.id_docusign,
                    int(proposta.tipo_pt),
                    proposta.versao_proposta,
                    proposta.vigencia,
                    proposta.fornecedor,
                    proposta.preposto,
                    proposta.email_preposto,
                    proposta.numero_proposta,
                    proposta.numero_proposta_comercial,
                    proposta.gestor_contratante,
                    proposta.horas_totais,
                    proposta.local_trabalho,
                    proposta.premissas,
                    proposta.dentro_escopo,
                    proposta.fora_escopo,
                    int(self._parse_documento_complementar(proposta.documento_complementar)),
                    proposta.aceite,
                ),
            )
            cursor.execute(
                """
                INSERT INTO tipo_proposta (
                    id_tipo_proposta, analise, requisitos, analise_programacao, testes, programacao, etl,
                    arquitetura, especificacao_execucao_testes, id_proposta
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                """,
                (
                    str(proposta.tipo_proposta.id_tipo_proposta),
                    int(proposta.tipo_proposta.analise),
                    int(proposta.tipo_proposta.requisitos),
                    int(proposta.tipo_proposta.analise_programacao),
                    int(proposta.tipo_proposta.testes),
                    int(proposta.tipo_proposta.programacao),
                    int(proposta.tipo_proposta.etl),
                    int(proposta.tipo_proposta.arquitetura),
                    int(proposta.tipo_proposta.especificacao_execucao_testes),
                    str(proposta.id_proposta),
                ),
            )
            total = 2
            for pacote in proposta.pacotes:
                cursor.execute(
                    """
                    INSERT INTO pacote (id_pacote, id_proposta, pacote, horas, data_ini, data_fim, perfil)
                    VALUES (?, ?, ?, ?, ?, ?, ?)
                    """,
                    (
                        str(pacote.id_pacote),
                        str(proposta.id_proposta),
                        pacote.pacote,
                        pacote.horas,
                        pacote.data_ini,
                        pacote.data_fim,
                        pacote.perfil,
                    ),
                )
                total += 1
            connection.commit()
            return total

    def _insert_proposta_sql_server(self, proposta: Proposta) -> int:
        with closing(pyodbc.connect(self._odbc_connection_string)) as connection:
            cursor = connection.cursor()
            cursor.execute(
                """
                INSERT INTO dbo.Proposta
                (
                    IdProposta, IdDocusign, TipoProposta, VersaoProposta, Vigencia, Fornecedor, Preposto,
                    EmailPreposto, NumeroProposta, NumeroPropostaComercial, HorasTotais, LocalTrabalho,
                    Premissas, DentroEscopo, ForaEscopo, DocumentoComplementar, Aceite
                )
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                """,
                (
                    str(proposta.id_proposta),
                    proposta.id_docusign,
                    int(proposta.tipo_pt),
                    proposta.versao_proposta,
                    proposta.vigencia,
                    proposta.fornecedor,
                    proposta.preposto,
                    proposta.email_preposto,
                    proposta.numero_proposta,
                    proposta.numero_proposta_comercial,
                    proposta.horas_totais,
                    proposta.local_trabalho,
                    proposta.premissas,
                    proposta.dentro_escopo,
                    proposta.fora_escopo,
                    self._parse_documento_complementar(proposta.documento_complementar),
                    proposta.aceite,
                ),
            )
            cursor.execute(
                """
                INSERT INTO dbo.TipoProposta
                (
                    IdTipoProposta, Analise, Requisitos, AnaliseProgramacao, Testes, Programacao, Etl,
                    Arquitetura, EspecificacaoExecucaoTestes, IdProposta
                )
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                """,
                (
                    str(proposta.tipo_proposta.id_tipo_proposta),
                    proposta.tipo_proposta.analise,
                    proposta.tipo_proposta.requisitos,
                    proposta.tipo_proposta.analise_programacao,
                    proposta.tipo_proposta.testes,
                    proposta.tipo_proposta.programacao,
                    proposta.tipo_proposta.etl,
                    proposta.tipo_proposta.arquitetura,
                    proposta.tipo_proposta.especificacao_execucao_testes,
                    str(proposta.id_proposta),
                ),
            )
            total = 2
            for pacote in proposta.pacotes:
                cursor.execute(
                    """
                    INSERT INTO dbo.Pacote (IdPacote, IdProposta, Horas, DataIni, DataFim)
                    VALUES (?, ?, ?, ?, ?)
                    """,
                    (
                        str(pacote.id_pacote),
                        str(proposta.id_proposta),
                        pacote.horas,
                        pacote.data_ini,
                        pacote.data_fim,
                    ),
                )
                total += 1
            connection.commit()
            return total

    def _insert_proposta_sql_server_pypyodbc(self, proposta: Proposta) -> int:
        connection = self._connect_pypyodbc()
        try:
            cursor = connection.cursor()
            cursor.execute(
                """
                INSERT INTO dbo.Proposta
                (
                    IdProposta, IdDocusign, TipoProposta, VersaoProposta, Vigencia, Fornecedor, Preposto,
                    EmailPreposto, NumeroProposta, NumeroPropostaComercial, HorasTotais, LocalTrabalho,
                    Premissas, DentroEscopo, ForaEscopo, DocumentoComplementar, Aceite
                )
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                """,
                (
                    str(proposta.id_proposta),
                    proposta.id_docusign,
                    int(proposta.tipo_pt),
                    proposta.versao_proposta,
                    proposta.vigencia,
                    proposta.fornecedor,
                    proposta.preposto,
                    proposta.email_preposto,
                    proposta.numero_proposta,
                    proposta.numero_proposta_comercial,
                    proposta.horas_totais,
                    proposta.local_trabalho,
                    proposta.premissas,
                    proposta.dentro_escopo,
                    proposta.fora_escopo,
                    self._parse_documento_complementar(proposta.documento_complementar),
                    proposta.aceite,
                ),
            )
            cursor.execute(
                """
                INSERT INTO dbo.TipoProposta
                (
                    IdTipoProposta, Analise, Requisitos, AnaliseProgramacao, Testes, Programacao, Etl,
                    Arquitetura, EspecificacaoExecucaoTestes, IdProposta
                )
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                """,
                (
                    str(proposta.tipo_proposta.id_tipo_proposta),
                    proposta.tipo_proposta.analise,
                    proposta.tipo_proposta.requisitos,
                    proposta.tipo_proposta.analise_programacao,
                    proposta.tipo_proposta.testes,
                    proposta.tipo_proposta.programacao,
                    proposta.tipo_proposta.etl,
                    proposta.tipo_proposta.arquitetura,
                    proposta.tipo_proposta.especificacao_execucao_testes,
                    str(proposta.id_proposta),
                ),
            )
            total = 2
            for pacote in proposta.pacotes:
                cursor.execute(
                    """
                    INSERT INTO dbo.Pacote (IdPacote, IdProposta, Horas, DataIni, DataFim)
                    VALUES (?, ?, ?, ?, ?)
                    """,
                    (
                        str(pacote.id_pacote),
                        str(proposta.id_proposta),
                        pacote.horas,
                        pacote.data_ini,
                        pacote.data_fim,
                    ),
                )
                total += 1
            connection.commit()
            return total
        except Exception:
            connection.rollback()
            raise
        finally:
            self._safe_close_pypyodbc(connection)

    def _insert_proposta_sql_server_pytds(self, proposta: Proposta) -> int:
        with closing(pytds.connect(**self._pytds_config, autocommit=False)) as connection:
            cursor = connection.cursor()
            cursor.execute(
                """
                INSERT INTO dbo.Proposta
                (
                    IdProposta, IdDocusign, TipoProposta, VersaoProposta, Vigencia, Fornecedor, Preposto,
                    EmailPreposto, NumeroProposta, NumeroPropostaComercial, HorasTotais, LocalTrabalho,
                    Premissas, DentroEscopo, ForaEscopo, DocumentoComplementar, Aceite
                )
                VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
                """,
                (
                    str(proposta.id_proposta),
                    proposta.id_docusign,
                    int(proposta.tipo_pt),
                    proposta.versao_proposta,
                    proposta.vigencia,
                    proposta.fornecedor,
                    proposta.preposto,
                    proposta.email_preposto,
                    proposta.numero_proposta,
                    proposta.numero_proposta_comercial,
                    proposta.horas_totais,
                    proposta.local_trabalho,
                    proposta.premissas,
                    proposta.dentro_escopo,
                    proposta.fora_escopo,
                    self._parse_documento_complementar(proposta.documento_complementar),
                    proposta.aceite,
                ),
            )
            cursor.execute(
                """
                INSERT INTO dbo.TipoProposta
                (
                    IdTipoProposta, Analise, Requisitos, AnaliseProgramacao, Testes, Programacao, Etl,
                    Arquitetura, EspecificacaoExecucaoTestes, IdProposta
                )
                VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
                """,
                (
                    str(proposta.tipo_proposta.id_tipo_proposta),
                    proposta.tipo_proposta.analise,
                    proposta.tipo_proposta.requisitos,
                    proposta.tipo_proposta.analise_programacao,
                    proposta.tipo_proposta.testes,
                    proposta.tipo_proposta.programacao,
                    proposta.tipo_proposta.etl,
                    proposta.tipo_proposta.arquitetura,
                    proposta.tipo_proposta.especificacao_execucao_testes,
                    str(proposta.id_proposta),
                ),
            )
            total = 2
            for pacote in proposta.pacotes:
                cursor.execute(
                    """
                    INSERT INTO dbo.Pacote (IdPacote, IdProposta, Horas, DataIni, DataFim)
                    VALUES (%s, %s, %s, %s, %s)
                    """,
                    (
                        str(pacote.id_pacote),
                        str(proposta.id_proposta),
                        pacote.horas,
                        pacote.data_ini,
                        pacote.data_fim,
                    ),
                )
                total += 1
            connection.commit()
            return total

    def _save_history_sqlite(self, item: ImportacaoHistoricoItem) -> None:
        with closing(self._sqlite_connection()) as connection:
            connection.execute(
                """
                INSERT INTO importacao_arquivo_historico (
                    id, nome_arquivo, origem, sucesso, registros_afetados, mensagem, data_processamento
                ) VALUES (?, ?, ?, ?, ?, ?, ?)
                """,
                (
                    str(item.id),
                    item.nome_arquivo,
                    item.origem,
                    int(item.sucesso),
                    item.registros_afetados,
                    item.mensagem,
                    item.data_processamento.isoformat(),
                ),
            )
            connection.commit()

    def _save_history_sql_server(self, item: ImportacaoHistoricoItem) -> None:
        with closing(pyodbc.connect(self._odbc_connection_string)) as connection:
            cursor = connection.cursor()
            cursor.execute(
                """
                INSERT INTO dbo.ImportacaoArquivoHistorico
                (Id, NomeArquivo, Origem, Sucesso, RegistrosAfetados, Mensagem, DataProcessamento)
                VALUES (?, ?, ?, ?, ?, ?, ?)
                """,
                (
                    str(item.id),
                    item.nome_arquivo,
                    item.origem,
                    item.sucesso,
                    item.registros_afetados,
                    item.mensagem,
                    item.data_processamento,
                ),
            )
            connection.commit()

    def _save_history_sql_server_pypyodbc(self, item: ImportacaoHistoricoItem) -> None:
        connection = self._connect_pypyodbc()
        try:
            cursor = connection.cursor()
            cursor.execute(
                """
                INSERT INTO dbo.ImportacaoArquivoHistorico
                (Id, NomeArquivo, Origem, Sucesso, RegistrosAfetados, Mensagem, DataProcessamento)
                VALUES (?, ?, ?, ?, ?, ?, ?)
                """,
                (
                    str(item.id),
                    item.nome_arquivo,
                    item.origem,
                    item.sucesso,
                    item.registros_afetados,
                    item.mensagem,
                    item.data_processamento,
                ),
            )
            connection.commit()
        finally:
            self._safe_close_pypyodbc(connection)

    def _save_history_sql_server_pytds(self, item: ImportacaoHistoricoItem) -> None:
        with closing(pytds.connect(**self._pytds_config, autocommit=False)) as connection:
            cursor = connection.cursor()
            cursor.execute(
                """
                INSERT INTO dbo.ImportacaoArquivoHistorico
                (Id, NomeArquivo, Origem, Sucesso, RegistrosAfetados, Mensagem, DataProcessamento)
                VALUES (%s, %s, %s, %s, %s, %s, %s)
                """,
                (
                    str(item.id),
                    item.nome_arquivo,
                    item.origem,
                    item.sucesso,
                    item.registros_afetados,
                    item.mensagem,
                    item.data_processamento,
                ),
            )
            connection.commit()

    def _list_history_sqlite(self, data_inicial: datetime | None, data_final: datetime | None) -> list[ImportacaoHistoricoItem]:
        query = """
            SELECT id, nome_arquivo, origem, sucesso, registros_afetados, mensagem, data_processamento
            FROM importacao_arquivo_historico
            WHERE (? IS NULL OR data_processamento >= ?)
              AND (? IS NULL OR data_processamento < ?)
            ORDER BY data_processamento DESC
        """
        data_final_exclusiva = (data_final + timedelta(days=1)) if data_final else None
        with closing(self._sqlite_connection()) as connection:
            rows = connection.execute(
                query,
                (
                    data_inicial.isoformat() if data_inicial else None,
                    data_inicial.isoformat() if data_inicial else None,
                    data_final_exclusiva.isoformat() if data_final_exclusiva else None,
                    data_final_exclusiva.isoformat() if data_final_exclusiva else None,
                ),
            ).fetchall()
        return [
            ImportacaoHistoricoItem(
                id=row["id"],
                nome_arquivo=row["nome_arquivo"],
                origem=row["origem"],
                sucesso=bool(row["sucesso"]),
                registros_afetados=row["registros_afetados"],
                mensagem=row["mensagem"],
                data_processamento=datetime.fromisoformat(row["data_processamento"]),
            )
            for row in rows
        ]

    def _list_history_sql_server(self, data_inicial: datetime | None, data_final: datetime | None) -> list[ImportacaoHistoricoItem]:
        query = """
            SELECT Id, NomeArquivo, Origem, Sucesso, RegistrosAfetados, Mensagem, CONCAT('', CONVERT(VARCHAR(33), DataProcessamento, 126)) AS DataProcessamento
            FROM dbo.ImportacaoArquivoHistorico
            WHERE (? IS NULL OR DataProcessamento >= ?)
              AND (? IS NULL OR DataProcessamento < ?)
            ORDER BY DataProcessamento DESC
        """
        data_final_exclusiva = (data_final + timedelta(days=1)) if data_final else None
        with closing(pyodbc.connect(self._odbc_connection_string)) as connection:
            cursor = connection.cursor()
            cursor.execute(query, data_inicial, data_inicial, data_final_exclusiva, data_final_exclusiva)
            rows = cursor.fetchall()
        return [
            ImportacaoHistoricoItem(
                id=row[0],
                nome_arquivo=row[1],
                origem=row[2],
                sucesso=bool(row[3]),
                registros_afetados=row[4],
                mensagem=row[5],
                data_processamento=row[6],
            )
            for row in rows
        ]

    def _list_history_sql_server_pypyodbc(self, data_inicial: datetime | None, data_final: datetime | None) -> list[ImportacaoHistoricoItem]:
        query = """
            SELECT Id, NomeArquivo, Origem, Sucesso, RegistrosAfetados, Mensagem, CONCAT('', CONVERT(VARCHAR(33), DataProcessamento, 126)) AS DataProcessamento
            FROM dbo.ImportacaoArquivoHistorico
            WHERE (? IS NULL OR DataProcessamento >= ?)
              AND (? IS NULL OR DataProcessamento < ?)
            ORDER BY DataProcessamento DESC
        """
        data_final_exclusiva = (data_final + timedelta(days=1)) if data_final else None
        connection = self._connect_pypyodbc()
        try:
            cursor = connection.cursor()
            cursor.execute(query, (data_inicial, data_inicial, data_final_exclusiva, data_final_exclusiva))
            rows = cursor.fetchall()
        finally:
            self._safe_close_pypyodbc(connection)
        return [
            ImportacaoHistoricoItem(
                id=self._coerce_text(row[0]),
                nome_arquivo=self._coerce_text(row[1]),
                origem=self._coerce_text(row[2]),
                sucesso=bool(row[3]),
                registros_afetados=row[4],
                mensagem=self._coerce_text(row[5]),
                data_processamento=self._parse_sqlserver_datetime_text(row[6]),
            )
            for row in rows
        ]

    def _list_history_sql_server_pytds(self, data_inicial: datetime | None, data_final: datetime | None) -> list[ImportacaoHistoricoItem]:
        query = """
            SELECT Id, NomeArquivo, Origem, Sucesso, RegistrosAfetados, Mensagem, DataProcessamento
            FROM dbo.ImportacaoArquivoHistorico
            WHERE (%s IS NULL OR DataProcessamento >= %s)
              AND (%s IS NULL OR DataProcessamento < %s)
            ORDER BY DataProcessamento DESC
        """
        data_final_exclusiva = (data_final + timedelta(days=1)) if data_final else None
        with closing(pytds.connect(**self._pytds_config, autocommit=False)) as connection:
            cursor = connection.cursor()
            cursor.execute(query, (data_inicial, data_inicial, data_final_exclusiva, data_final_exclusiva))
            rows = cursor.fetchall()
        return [
            ImportacaoHistoricoItem(
                id=row[0],
                nome_arquivo=row[1],
                origem=row[2],
                sucesso=bool(row[3]),
                registros_afetados=row[4],
                mensagem=row[5],
                data_processamento=row[6],
            )
            for row in rows
        ]

    @staticmethod
    def _parse_documento_complementar(valor: str) -> bool:
        texto = (valor or "").strip().lower()
        return texto in {"1", "true", "sim", "s", "yes", "y"} or "x" in texto

    @staticmethod
    def _build_odbc_connection_string(database_url: str) -> str:
        parsed = urlparse(database_url)
        username = unquote(parsed.username or "")
        password = unquote(parsed.password or "")
        host = parsed.hostname or "localhost"
        port = parsed.port or 1433
        database = parsed.path.lstrip("/")
        query = parse_qs(parsed.query)
        driver = unquote(query.get("driver", ["ODBC Driver 18 for SQL Server"])[0])
        trust = query.get("TrustServerCertificate", ["yes"])[0]
        encrypt = query.get("Encrypt", ["yes"])[0]
        return (
            f"DRIVER={{{driver}}};"
            f"SERVER={host},{port};"
            f"DATABASE={database};"
            f"UID={username};"
            f"PWD={password};"
            f"Encrypt={encrypt};"
            f"TrustServerCertificate={trust};"
        )

    @staticmethod
    def _build_pytds_config(database_url: str) -> dict[str, object]:
        parsed = urlparse(database_url)
        query = parse_qs(parsed.query)
        return {
            "server": parsed.hostname or "localhost",
            "port": parsed.port or 1433,
            "database": parsed.path.lstrip("/"),
            "user": unquote(parsed.username or ""),
            "password": unquote(parsed.password or ""),
            "login_timeout": float(query.get("login_timeout", ["30"])[0]),
            "timeout": float(query.get("timeout", ["30"])[0]),
            "cafile": DatabaseRepository._resolve_cafile(query.get("cafile", [None])[0]),
            "validate_host": query.get("validate_host", ["true"])[0].lower() == "true",
            "enc_login_only": query.get("enc_login_only", ["false"])[0].lower() == "true",
        }

    @staticmethod
    def _resolve_cafile(value: str | None) -> str | None:
        if not value:
            return None
        if value == "certifi":
            if certifi is None:
                raise ValueError("certifi nao esta instalado para fornecer a cadeia TLS do SQL Server.")
            return certifi.where()
        return value

    @staticmethod
    def _parse_sqlserver_datetime_text(value: str) -> datetime:
        texto = (value or "").strip()
        if "." not in texto:
            return datetime.fromisoformat(texto)
        base, fractional = texto.split(".", 1)
        fractional = (fractional + "000000")[:6]
        return datetime.fromisoformat(f"{base}.{fractional}")

    @staticmethod
    def _safe_close_pypyodbc(connection) -> None:
        try:
            connection.close()
        except Exception:
            pass

    def _connect_pypyodbc(self, autocommit: bool = False):
        connection = pypyodbc.connect(self._pypyodbc_connection_string, autocommit=autocommit)
        connection.add_output_converter(pypyodbc.SQL_TIMESTAMP, self._safe_pypyodbc_datetime)
        connection.add_output_converter(pypyodbc.SQL_TYPE_TIMESTAMP, self._safe_pypyodbc_datetime)
        return connection

    @staticmethod
    def _safe_pypyodbc_datetime(value):
        texto = value.decode("utf-8") if isinstance(value, (bytes, bytearray)) else str(value)
        return DatabaseRepository._parse_sqlserver_datetime_text(texto)

    @staticmethod
    def _coerce_text(value) -> str:
        if isinstance(value, (bytes, bytearray)):
            return value.decode("utf-8")
        texto = str(value)
        if texto.startswith("b'") and texto.endswith("'"):
            return texto[2:-1]
        return texto
