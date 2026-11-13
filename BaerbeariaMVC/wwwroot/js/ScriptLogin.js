function validarLogin() {
    const usuarioDigitado = document.getElementById('usuario').value;
    const senhaDigitada = document.getElementById('senha').value;

    if (!usuarioDigitado || !senhaDigitada) {
        alert("Por favor, preencha todos os campos.");
        return;
    }

    // Monta o pacote com o mesmo nome das propriedades da classe C#
    const dadosLogin = {
        Usuario: usuarioDigitado,
        Senha: senhaDigitada
    };

    fetch('http://localhost:5000/api/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(dadosLogin)
    })
        .then(async resposta => {
            if (!resposta.ok) {
                const erroJson = await resposta.json();
                throw new Error(erroJson.mensagem);
            }
            return resposta.json();
        })
        .then(dados => {
            alert(dados.mensagem);
            // Redireciona para o painel se a base de dados validar
            window.location.href = "/Front-end/pages/admin.html";
        })
        .catch(erro => {
            document.getElementById('senha').value = '';
            alert(erro.message);
        });
}