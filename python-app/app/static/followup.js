const dataInicial = document.getElementById("dataInicial");
const dataFinal = document.getElementById("dataFinal");
const statusFiltro = document.getElementById("statusFiltro");
const followupError = document.getElementById("followup-error");
const followupStatus = document.getElementById("followup-status");
const followupBody = document.getElementById("followup-body");

function todayString() {
    return new Date().toISOString().slice(0, 10);
}

function queryString() {
    const params = new URLSearchParams();
    if (dataInicial.value) params.set("dataInicial", dataInicial.value);
    if (dataFinal.value) params.set("dataFinal", dataFinal.value);
    return params.toString();
}

function updateCards(items) {
    const filtrados = items.filter((item) => {
        if (statusFiltro.value === "Sucesso") return item.sucesso;
        if (statusFiltro.value === "Falha") return !item.sucesso;
        return true;
    });

    const total = filtrados.length;
    const totalSucesso = filtrados.filter((item) => item.sucesso).length;
    const totalFalha = filtrados.filter((item) => !item.sucesso).length;
    const percentualSucesso = total ? (totalSucesso / total) * 100 : 0;
    const percentualFalha = total ? (totalFalha / total) * 100 : 0;

    document.getElementById("totalImportacoes").textContent = total;
    document.getElementById("percentualSucesso").textContent = `${percentualSucesso.toFixed(1)}%`;
    document.getElementById("percentualFalha").textContent = `${percentualFalha.toFixed(1)}%`;
    document.getElementById("totalSucessoLabel").textContent = `${totalSucesso} importacoes concluidas com sucesso.`;
    document.getElementById("totalFalhaLabel").textContent = `${totalFalha} importacoes com falha.`;
    document.getElementById("resumoSucesso").textContent = totalSucesso;
    document.getElementById("resumoFalha").textContent = totalFalha;
    document.getElementById("resumoTotal").textContent = total;
    document.getElementById("donut-total").textContent = total;
    document.getElementById("donut-success").setAttribute("stroke-dasharray", `${percentualSucesso} ${100 - percentualSucesso}`);
    document.getElementById("donut-failure").setAttribute("stroke-dasharray", `${percentualFalha} ${100 - percentualFalha}`);
    document.getElementById("donut-failure").setAttribute("stroke-dashoffset", `${25 - percentualSucesso}`);

    if (!filtrados.length) {
        followupStatus.textContent = "Nenhum arquivo encontrado para o filtro informado.";
        followupStatus.classList.remove("hidden");
    } else {
        followupStatus.classList.add("hidden");
    }

    followupBody.innerHTML = filtrados.map((item) => `
        <tr>
            <td>${new Date(item.data_processamento).toLocaleString("pt-BR")}</td>
            <td>${item.nome_arquivo}</td>
            <td>${item.origem}</td>
            <td><span class="badge ${item.sucesso ? "badge-success" : "badge-error"}">${item.sucesso ? "Sucesso" : "Falhou"}</span></td>
            <td>${item.registros_afetados}</td>
            <td>${item.mensagem || ""}</td>
        </tr>
    `).join("");
}

async function carregar() {
    followupError.classList.add("hidden");
    followupStatus.textContent = "Carregando historico...";
    followupStatus.classList.remove("hidden");

    try {
        const response = await fetch(`/api/importacoes/historico?${queryString()}`);
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }
        const items = await response.json();
        updateCards(items);
    } catch (error) {
        followupError.textContent = `Nao foi possivel carregar o historico: ${error.message}`;
        followupError.classList.remove("hidden");
        followupStatus.textContent = "Falha ao carregar historico.";
    }
}

function limpar() {
    dataInicial.value = "";
    dataFinal.value = "";
    statusFiltro.value = "Todos";
    carregar();
}

function exportar() {
    window.location.href = `/api/importacoes/exportar?${queryString()}`;
}

dataInicial.value = todayString();
dataFinal.value = todayString();
document.getElementById("btn-filtrar").addEventListener("click", carregar);
document.getElementById("btn-limpar").addEventListener("click", limpar);
document.getElementById("btn-exportar").addEventListener("click", exportar);
statusFiltro.addEventListener("change", carregar);

carregar();
setInterval(carregar, 15000);
