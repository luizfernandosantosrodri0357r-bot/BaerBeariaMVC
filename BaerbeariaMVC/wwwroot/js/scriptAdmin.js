// Assim que a página de admin abrir, ele roda essa função
// ==========================================
// PARTE 1: GERENCIAMENTO DE AGENDAMENTOS (COM FILTRO)
// ==========================================

function carregarAgenda() {
    // Pega a data que o admin selecionou no filtro (se houver)
    const dataFiltro = document.getElementById('filtro-data')?.value;

    fetch('http://localhost:5000/api/agendamentos')
        .then(resposta => resposta.json())
        .then(listaDeAgendamentos => {

            const corpoTabela = document.getElementById('tabela-agendamentos-body');
            if (!corpoTabela) return;
            corpoTabela.innerHTML = '';

            // SE TIVER DATA NO FILTRO, ELE FILTRA A LISTA AQUI!
            let listaFiltrada = listaDeAgendamentos;
            if (dataFiltro) {
                listaFiltrada = listaDeAgendamentos.filter(agendamento => {
                    const dataDoBanco = agendamento.dataAgendada.split('T')[0]; // Pega só o YYYY-MM-DD
                    return dataDoBanco === dataFiltro;
                });
            }

            // Monta a tabela apenas com os registros filtrados
            listaFiltrada.forEach(agendamento => {
                let dataFormatada = new Date(agendamento.dataAgendada).toLocaleDateString('pt-BR');
                let idCorreto = agendamento.id !== undefined ? agendamento.id : agendamento.Id;

                let colunaAcoes = '';
                if (agendamento.situacao === 'Pendente') {
                    colunaAcoes = `
                        <button class="btn-acao btn-salvar" onclick="atualizarStatus(${idCorreto}, 'Concluido')">✔ Concluir</button>
                        <button class="btn-acao btn-cancelar" onclick="atualizarStatus(${idCorreto}, 'Cancelado')">✖ Cancelar</button>
                    `;
                } else if (agendamento.situacao === 'Concluido' || agendamento.situacao === 'Concluído') {
                    colunaAcoes = `<span style="color: green; font-weight: bold;">✔ Concluído</span>`;
                } else {
                    colunaAcoes = `<span style="color: red; font-weight: bold;">✖ Cancelado</span>`;
                }

                corpoTabela.innerHTML += `
                    <tr>
                        <td>${dataFormatada}</td>
                        <td>${agendamento.horario}</td>
                        <td>${agendamento.nomeCliente}</td>
                        <td>${agendamento.whatsApp}</td>
                        <td>${colunaAcoes}</td>
                    </tr>
                `;
            });
        })
        .catch(erro => console.error("Erro ao buscar a agenda:", erro));
}

// Função para limpar o campo de data e mostrar toda a agenda novamente
function limparFiltro() {
    const campoFiltro = document.getElementById('filtro-data');
    if (campoFiltro) {
        campoFiltro.value = ''; // Esvazia a data
        carregarAgenda(); // Recarrega tudo
    }
}

// OUVINTE DE EVENTO: Sempre que o admin mudar a data no filtro, roda a função automaticamente
document.addEventListener('DOMContentLoaded', () => {
    const campoFiltro = document.getElementById('filtro-data');
    if (campoFiltro) {
        campoFiltro.addEventListener('change', carregarAgenda);
    }
});

// FUNÇÃO NOVA: Comunica com o C# para mudar o status
function atualizarStatus(idAgendamento, novoStatus) {
    // Pede uma confirmação antes de alterar
    if (!confirm(`Tem certeza que deseja marcar este agendamento como ${novoStatus}?`)) {
        return;
    }

    // Chama a rota nova que acabamos de criar no C# (usando PUT)
    fetch(`http://localhost:5000/api/agendamentos/${idAgendamento}/${novoStatus}`, {
        method: 'PUT'
    })
        .then(resposta => resposta.json())
        .then(dados => {
            // Mostra a mensagem de sucesso que veio do C#
            alert(dados.mensagem);
            // Recarrega a tabela automaticamente para atualizar a tela!
            carregarAgenda();
        })
        .catch(erro => {
            console.error(erro);
            alert('Erro ao atualizar o agendamento.');
        });
}

// Chama a função inicial
carregarAgenda();

// ==========================================
// GERENCIAMENTO DE SERVIÇOS
// ==========================================

// Função para buscar os serviços no C# e montar a tabela
function carregarServicos() {
    fetch('http://localhost:5000/api/servicos')
        .then(resposta => resposta.json())
        .then(listaDeServicos => {
            const corpoTabela = document.getElementById('tabela-servicos-body');
            corpoTabela.innerHTML = '';

            listaDeServicos.forEach(servico => {
                let id = servico.id !== undefined ? servico.id : servico.Id;
                let nome = servico.nome !== undefined ? servico.nome : servico.Nome;
                let preco = servico.preco !== undefined ? servico.preco : servico.Preco;

                // Cria a linha com os inputs já preenchidos
                corpoTabela.innerHTML += `
                    <tr id="linha-servico-${id}">
                        <td><input type="text" id="nome-servico-${id}" value="${nome}" class="input-editavel"></td>
                        <td><input type="number" id="preco-servico-${id}" value="${preco.toFixed(2)}" class="input-editavel" step="0.50"></td>
                        <td>
                            <button class="btn-acao btn-salvar" onclick="salvarServico(${id})">Salvar</button>
                            <button class="btn-acao btn-cancelar" onclick="excluirServico(${id})">Excluir</button>
                        </td>
                    </tr>
                `;
            });
        })
        .catch(erro => console.error("Erro ao buscar serviços:", erro));
}

// Cria uma linha vazia no topo da tabela para o usuário digitar um novo serviço
function criarLinhaNovoServico() {
    const corpoTabela = document.getElementById('tabela-servicos-body');

    // O ID 0 serve para o sistema saber que é um cadastro novo
    const linhaNova = `
        <tr id="linha-servico-0" style="background-color: #f4f4f4;">
            <td><input type="text" id="nome-servico-0" placeholder="Nome do serviço" class="input-editavel"></td>
            <td><input type="number" id="preco-servico-0" placeholder="0.00" class="input-editavel" step="0.50"></td>
            <td>
                <button class="btn-acao btn-salvar" onclick="salvarServico(0)">✔ Confirmar</button>
                <button class="btn-acao btn-cancelar" onclick="carregarServicos()">✖ Cancelar</button>
            </td>
        </tr>
    `;

    // Insere no começo da tabela
    corpoTabela.insertAdjacentHTML('afterbegin', linhaNova);
}

// Função inteligente que serve tanto para CRIAR quanto para ATUALIZAR
function salvarServico(id) {
    const nomeInput = document.getElementById(`nome-servico-${id}`).value;
    const precoInput = document.getElementById(`preco-servico-${id}`).value;

    if (!nomeInput || !precoInput) {
        alert("Por favor, preencha o nome e o preço do serviço.");
        return;
    }

    // Se o ID for 0, é um serviço novo (POST). Se tiver ID, é atualização (PUT).
    const url = id === 0 ? 'http://localhost:5000/api/servicos' : `http://localhost:5000/api/servicos/${id}`;
    const metodoHTTP = id === 0 ? 'POST' : 'PUT';

    const pacoteDeDados = {
        Nome: nomeInput,
        Preco: parseFloat(precoInput)
    };

    fetch(url, {
        method: metodoHTTP,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(pacoteDeDados)
    })
        .then(async resposta => {
            // Se o C# devolver o Erro 400 (Bad Request), nós pegamos a sua frase aqui!
            if (!resposta.ok) {
                const erroJson = await resposta.json();
                throw new Error(erroJson.mensagem);
            }
            return resposta.json();
        })
        .then(dados => {
            alert(dados.mensagem); // Mensagem de Sucesso
            carregarServicos();
        })
        .catch(erro => {
            // Exibe o seu erro de encapsulamento direto na tela do usuário!
            alert(erro.message);
        });
}

// Comunica com o C# para deletar do banco
function excluirServico(id) {
    if (!confirm("Atenção: Tem certeza que deseja excluir este serviço do catálogo?")) {
        return;
    }

    fetch(`http://localhost:5000/api/servicos/${id}`, {
        method: 'DELETE'
    })
        .then(resposta => resposta.json())
        .then(dados => {
            alert(dados.mensagem);
            carregarServicos(); // Recarrega a tabela
        })
        .catch(erro => console.error("Erro ao excluir:", erro));
}

// Chama a função para carregar os serviços assim que a página abre
carregarServicos();