from enum import IntEnum


class TipoPt(IntEnum):
    PROJETO = 1
    ALOCACAO = 2

    @property
    def label(self) -> str:
        return "Projeto" if self == TipoPt.PROJETO else "Alocacao"
