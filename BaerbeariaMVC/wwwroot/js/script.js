// ==========================================
// CARREGAR SERVIÇOS NA TELA DO CLIENTE
// ==========================================
function carregarServicosVitrine() {
    fetch('http://localhost:5000/api/servicos')
        .then(resposta => resposta.json())
        .then(listaDeServicos => {

            const vitrine = document.getElementById('vitrine-servicos');
            vitrine.innerHTML = ''; // Limpa a div antes de carregar

            listaDeServicos.forEach(servico => {
                // Garante que vai ler a propriedade certa independente de maiúscula/minúscula
                let nome = servico.nome !== undefined ? servico.nome : servico.Nome;
                let preco = servico.preco !== undefined ? servico.preco : servico.Preco;

                // Converte o número (ex: 35.00) para Moeda (ex: R$ 35,00)
                let precoFormatado = preco.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

                // Injeta o HTML com as classes originais do seu CSS
                vitrine.innerHTML += `
                    <div class="item-servico">
                        <span class="servico-nome">${nome}</span>
                        <span class="servico-linha"></span>
                        <span class="servico-preco">${precoFormatado}</span>
                    </div>
                `;
            });
        })
        .catch(erro => console.error("Erro ao buscar serviços da vitrine:", erro));
}

// Chama a função automaticamente assim que o index.html for aberto
carregarServicosVitrine();

// Impede selecionar datas passadas
const dataInput = document.getElementById('data');
const hoje = new Date().toISOString().split('T')[0];
dataInput.setAttribute('min', hoje);

// Função definitiva da máscara de WhatsApp
function mascaraTelefone(event) {
    let input = event.target;

    // Tira tudo que não é número na mesma hora
    let valor = input.value.replace(/\D/g, '');

    // Trava absoluta de 11 números (2 do DDD + 9 do telefone)
    if (valor.length > 11) {
        valor = valor.substring(0, 11);
    }

    // Remonta a máscara bonitinha
    if (valor.length > 2) {
        valor = valor.replace(/^(\d{2})(\d)/g, '($1) $2');
    }
    if (valor.length > 9) {
        valor = valor.replace(/(\d)(\d{4})$/, '$1-$2');
    }

    // Devolve para o HTML já formatado e travado
    input.value = valor;
}

// Captura o formulário do HTML
const formAgendamento = document.querySelector('.formulario-agendamento');

if (formAgendamento) {
    formAgendamento.addEventListener('submit', function (event) {
        // 1. IMPEDE A PÁGINA DE RECARREGAR E SUMIR COM O ERRO
        event.preventDefault();

        try {
            // 2. CAPTURA E VALIDA O HORÁRIO (RADIO BUTTON)
            const horarioMarcado = document.querySelector('input[name="horario"]:checked');
            if (!horarioMarcado) {
                alert("Por favor, selecione um horário disponível.");
                return;
            }

            // 3. CAPTURA E VALIDA O SERVIÇO SELECIONADO
            const selectServico = document.getElementById('servicoSelecionado');
            if (!selectServico || !selectServico.value) {
                alert("Por favor, selecione um serviço.");
                return;
            }

            // 4. MONTA O OBJETO EXATAMENTE COMO A SUA CLASSE C# ESPERA
            // (Ajuste o nome 'ServicoId' ou 'CodServico' de acordo com a sua classe Agendamento no C#)
            const dadosDoAgendamento = {
                DataAgendada: document.getElementById('data').value,
                Horario: horarioMarcado.value,
                NomeCliente: document.getElementById('nome').value,
                WhatsApp: document.getElementById('whatsapp').value,

                // CORRIGIDO: Troque de 'ServicoId' para o nome exato da propriedade no C#
                CodServico: parseInt(selectServico.value)
            };

            // 5. ENVIA PARA A API EM C#
            fetch('http://localhost:5000/api/agendamentos', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(dadosDoAgendamento)
            })
                .then(async resposta => {
                    // Se o C# recusar (ex: horário ocupado), captura a mensagem tratada
                    if (!resposta.ok) {
                        const erroJson = await resposta.json();
                        throw new Error(erroJson.mensagem);
                    }
                    return resposta.json();
                })
                .then(dados => {
                    // Se der certo, avisa o cliente e limpa a tela
                    alert(dados.mensagem);
                    window.location.reload();
                })
                .catch(erro => {
                    // Exibe o alerta caso o horário já esteja ocupado
                    alert(erro.message);
                });

        } catch (error) {
            // Se qualquer outra coisa quebrar no JS, esse alert vai te avisar o motivo!
            console.error("Erro interno no JavaScript:", error);
            alert("Erro ao processar o formulário: " + error.message);
        }
    });
}
// Função para formatar o WhatsApp enquanto o cliente digita
function mascaraTelefone(event) {
    let input = event.target;
    let valor = input.value.replace(/\D/g, ''); // Tira tudo que não é número

    if (valor.length > 0) {
        valor = valor.replace(/^(\d{2})(\d)/g, '($1) $2'); // Coloca os parênteses no DDD
        valor = valor.replace(/(\d)(\d{4})$/, '$1-$2');    // Coloca o traço no número
    }

    input.value = valor;
}

// ==========================================
// CONTROLE DE HORÁRIOS OCUPADOS EM TEMPO REAL
// ==========================================
const inputData = document.getElementById('data');

if (inputData) {
    // Sempre que o cliente trocar a data, executa a checagem
    inputData.addEventListener('change', function () {
        const dataSelecionada = inputData.value;
        if (!dataSelecionada) return;

        // 1. RESET: Antes de checar, limpa os bloqueios do dia anterior
        const todosOsCards = document.querySelectorAll('.card-horario');
        todosOsCards.forEach(card => {
            card.classList.remove('ocupado');
            card.querySelector('input').disabled = false;

            // Se o horário que estava marcado agora for bloqueado, desmarca ele
            if (card.querySelector('input').checked) {
                card.querySelector('input').checked = false;
            }
        });

        // 2. BUSCA NO C#: Pega todos os agendamentos do banco
        fetch('http://localhost:5000/api/agendamentos')
            .then(resposta => resposta.json())
            .then(agendamentos => {

                // Filtra os agendamentos que são da mesma data e não estão cancelados
                const agendamentosDoDia = agendamentos.filter(a => {
                    const dataDoBanco = a.dataAgendada.split('T')[0]; // Pega só o YYYY-MM-DD
                    return dataDoBanco === dataSelecionada && a.situacao !== 'Cancelado';
                });

                // 3. BLOQUEIO VISUAL: Risca e desabilita os horários batidos
                agendamentosDoDia.forEach(agendamento => {
                    // Procura o input radio que tem o mesmo valor do horário (ex: "10:00")
                    const inputOcupado = document.querySelector(`input[name="horario"][value="${agendamento.horario}"]`);

                    if (inputOcupado) {
                        inputOcupado.disabled = true; // Desabilita o rádio
                        inputOcupado.parentElement.classList.add('ocupado'); // Adiciona o CSS vermelho/riscado no Label pai
                    }
                });
            })
            .catch(erro => console.error("Erro ao atualizar grade de horários:", erro));
    });
}

// ==========================================
// CARREGAR OPÇÕES DE SERVIÇO NO FORMULÁRIO
// ==========================================
function carregarOpcoesDeServico() {
    fetch('http://localhost:5000/api/servicos')
        .then(resposta => resposta.json())
        .then(listaDeServicos => {
            const select = document.getElementById('servicoSelecionado');

            // Texto padrão antes do cliente clicar
            select.innerHTML = '<option value="" disabled selected>Selecione um serviço...</option>';

            listaDeServicos.forEach(servico => {
                let id = servico.id !== undefined ? servico.id : servico.Id;
                let nome = servico.nome !== undefined ? servico.nome : servico.Nome;
                let preco = servico.preco !== undefined ? servico.preco : servico.Preco;

                let precoFormatado = preco.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

                // Cria a opção. O 'value' é o ID que vai para o Banco de Dados!
                select.innerHTML += `<option value="${id}">${nome} - ${precoFormatado}</option>`;
            });
        })
        .catch(erro => console.error("Erro ao carregar o select:", erro));
}

// Chama a função para preencher o select ao abrir a página
carregarOpcoesDeServico();
