const globalError = document.getElementById("global-error");
const globalSuccess = document.getElementById("global-success");
const tipoPt = document.getElementById("tipoPt");

function showMessage(target, message) {
    target.textContent = message;
    target.classList.remove("hidden");
}

function hideMessage(target) {
    target.textContent = "";
    target.classList.add("hidden");
}

function setProgress(prefixo, status, percentual) {
    document.getElementById(`status-${prefixo}`).textContent = status;
    document.getElementById(`percentual-${prefixo}`).textContent = `${percentual}%`;
    document.getElementById(`barra-${prefixo}`).style.width = `${percentual}%`;
}

function renderResultadoUnico(resultado) {
    const target = document.getElementById("resultado-unico");
    target.className = `result-box ${resultado.sucesso ? "result-success" : "result-error"}`;
    target.innerHTML = `
        <strong>${resultado.nome_arquivo}</strong>
        <span>Tipo: ${resultado.tipo_pt_label || ""}</span>
        <span>Status: ${resultado.sucesso ? "Sucesso" : "Falhou"}</span>
        <span>Registros: ${resultado.registros_afetados}</span>
        <span>${resultado.mensagem || ""}</span>
    `;
}

function renderResultadoLote(id, resultado) {
    const target = document.getElementById(id);
    target.classList.remove("hidden");
    target.innerHTML = `
        <span>Tipo: ${resultado.tipo_pt_label || ""}</span>
        <span>Total: ${resultado.total_arquivos}</span>
        <span>Sucesso: ${resultado.arquivos_processados_com_sucesso}</span>
        <span>Falha: ${resultado.arquivos_com_erro}</span>
    `;
}

async function importarArquivo() {
    hideMessage(globalError);
    hideMessage(globalSuccess);
    const input = document.getElementById("arquivo-unico");
    const file = input.files[0];
    if (!file) {
        showMessage(globalError, "Selecione um arquivo PDF.");
        return;
    }

    setProgress("unico", "Enviando arquivo...", 45);
    const formData = new FormData();
    formData.append("arquivo", file);

    try {
        const response = await fetch(`/api/propostas/importar?tipoPt=${tipoPt.value}`, {
            method: "POST",
            body: formData,
        });
        const body = await response.json();
        if (!response.ok) {
            throw new Error(body.mensagem || JSON.stringify(body));
        }
        setProgress("unico", "Execucao concluida", 100);
        renderResultadoUnico(body);
        showMessage(globalSuccess, "Importacao concluida. O follow-up sera atualizado automaticamente na outra tela.");
    } catch (error) {
        setProgress("unico", "Falha na execucao", 0);
        showMessage(globalError, `Falha ao executar importacao unica: ${error.message}`);
    }
}

async function importarLote() {
    hideMessage(globalError);
    hideMessage(globalSuccess);
    const input = document.getElementById("arquivos-lote");
    const files = Array.from(input.files);
    if (!files.length) {
        showMessage(globalError, "Selecione ao menos um arquivo PDF para o lote.");
        return;
    }

    const formData = new FormData();
    files.forEach((file) => formData.append("arquivos", file));
    document.getElementById("lote-atual").textContent = "";

    try {
        setProgress("lote", `Processando 1 de ${files.length}...`, 10);
        const response = await fetch(`/api/propostas/importar-lote?tipoPt=${tipoPt.value}`, {
            method: "POST",
            body: formData,
        });
        const body = await response.json();
        if (!response.ok) {
            throw new Error(body.mensagem || JSON.stringify(body));
        }
        setProgress("lote", "Execucao concluida", 100);
        renderResultadoLote("resultado-lote", body);
        showMessage(globalSuccess, "Importacao em lote concluida. O follow-up sera atualizado automaticamente na outra tela.");
    } catch (error) {
        setProgress("lote", "Falha na execucao", 0);
        showMessage(globalError, `Falha ao executar importacao em lote: ${error.message}`);
    }
}

async function importarPasta() {
    hideMessage(globalError);
    hideMessage(globalSuccess);
    const caminho = document.getElementById("caminho-pasta").value.trim();
    if (!caminho) {
        showMessage(globalError, "Informe um caminho de pasta.");
        return;
    }

    try {
        setProgress("pasta", "Executando importacao por pasta...", 45);
        const response = await fetch(`/api/propostas/importar-pasta?tipoPt=${tipoPt.value}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ caminhoPasta: caminho }),
        });
        const body = await response.json();
        if (!response.ok) {
            throw new Error(body.mensagem || JSON.stringify(body));
        }
        setProgress("pasta", "Execucao concluida", 100);
        renderResultadoLote("resultado-pasta", body);
        showMessage(globalSuccess, "Importacao por pasta concluida. O follow-up sera atualizado automaticamente na outra tela.");
    } catch (error) {
        setProgress("pasta", "Falha na execucao", 0);
        showMessage(globalError, `Falha ao executar importacao por pasta: ${error.message}`);
    }
}

document.getElementById("arquivo-unico").addEventListener("change", (event) => {
    document.getElementById("arquivo-unico-helper").textContent = event.target.files[0]?.name || "";
});

document.getElementById("arquivos-lote").addEventListener("change", (event) => {
    document.getElementById("lote-helper").textContent = `${event.target.files.length || 0} arquivo(s) selecionado(s)`;
});

document.getElementById("btn-arquivo-unico").addEventListener("click", importarArquivo);
document.getElementById("btn-lote").addEventListener("click", importarLote);
document.getElementById("btn-pasta").addEventListener("click", importarPasta);
