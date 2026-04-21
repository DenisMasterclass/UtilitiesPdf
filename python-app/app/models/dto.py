from __future__ import annotations

from datetime import datetime
from typing import List
from uuid import UUID, uuid4

from pydantic import BaseModel, Field

from app.models.types import TipoPt


class TipoPropostaFlags(BaseModel):
    id_tipo_proposta: UUID = Field(default_factory=uuid4)
    analise: bool = False
    requisitos: bool = False
    testes: bool = False
    programacao: bool = False
    analise_programacao: bool = False
    etl: bool = False
    arquitetura: bool = False
    especificacao_execucao_testes: bool = False


class Pacote(BaseModel):
    id_pacote: UUID = Field(default_factory=uuid4)
    id_proposta: UUID | None = None
    pacote: str = ""
    horas: str = ""
    data_ini: str = ""
    data_fim: str = ""
    perfil: str = ""


class Proposta(BaseModel):
    id_proposta: UUID = Field(default_factory=uuid4)
    id_docusign: str = ""
    tipo_pt: TipoPt
    versao_proposta: str = ""
    vigencia: str = ""
    fornecedor: str = ""
    preposto: str = ""
    email_preposto: str = ""
    numero_proposta: str = ""
    numero_proposta_comercial: str = ""
    gestor_contratante: str = ""
    horas_totais: float = 0
    local_trabalho: str = ""
    premissas: str = ""
    dentro_escopo: str = ""
    fora_escopo: str = ""
    documento_complementar: str = ""
    aceite: str = ""
    tipo_proposta: TipoPropostaFlags = Field(default_factory=TipoPropostaFlags)
    pacotes: List[Pacote] = Field(default_factory=list)


class ImportacaoPdfResultado(BaseModel):
    nome_arquivo: str = ""
    sucesso: bool
    registros_afetados: int = 0
    mensagem: str | None = None
    tipo_pt: TipoPt | None = None
    tipo_pt_label: str = ""


class ImportacaoPdfLoteResultado(BaseModel):
    total_arquivos: int = 0
    arquivos_processados_com_sucesso: int = 0
    arquivos_com_erro: int = 0
    tipo_pt: TipoPt | None = None
    tipo_pt_label: str = ""
    resultados: List[ImportacaoPdfResultado] = Field(default_factory=list)


class ImportacaoHistoricoItem(BaseModel):
    id: UUID = Field(default_factory=uuid4)
    nome_arquivo: str
    origem: str
    sucesso: bool
    registros_afetados: int = 0
    mensagem: str = ""
    data_processamento: datetime = Field(default_factory=datetime.now)


class ImportarPropostasPorPastaRequest(BaseModel):
    caminho_pasta: str
