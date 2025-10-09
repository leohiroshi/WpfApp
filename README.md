# WpfApp

Aplicação desktop em C# com WPF (.NET 8) para cadastro e gestão de Pessoas, Produtos e Pedidos.
Persistência em arquivos JSON (Newtonsoft.Json) e uso de LINQ, seguindo o padrão MVVM.

Status: Pronto para uso

## Requisitos
- Windows 10/11
- Visual Studio 2022 (ou VS Code com C# Dev Kit)
- .NET 8 SDK

Pacotes NuGet principais:
- MahApps.Metro.IconPacks.Material
- Newtonsoft.Json

## Como executar
1) Clonar o repositório
- Via Git GUI ou terminal

2) Abrir a solução
- Abra `WpfApp.sln` no Visual Studio 2022

3) Restaurar pacotes
- O Visual Studio restaura automaticamente ao abrir; se necessário, use Restore NuGet Packages

4) Rodar o projeto
- Defina `WpfApp` como projeto de inicialização
- Pressione F5 (Debug) ou Ctrl+F5 (Sem Debug)

Observações:
- Os arquivos de dados em `Data/` (pessoas.json, produtos.json, pedidos.json) são copiados para a pasta de saída automaticamente (CopyToOutputDirectory=PreserveNewest)
- Caso deseje iniciar “zerado”, apague os JSONs da pasta `bin/Debug/net8.0-windows/Data` com a aplicação fechada

## Estrutura de pastas
WpfApp/
├─ Models/        (Domínio: Pessoa, Produto, Pedido, enums)
├─ ViewModels/    (MVVM: PessoasViewModel, ProdutosViewModel, PedidosViewModel, BaseViewModel)
├─ Views/         (XAML da janela principal e modais)
├─ Services/      (Repos: PessoaRepository, ProdutoRepository, PedidoRepository; PedidoService)
├─ Helpers/       (Infra: RelayCommand, validadores, etc.)
├─ Data/          (Arquivos .json de dados)
├─ Resources/     (Estilos, imagens, ícones)
└─ WpfApp.csproj  (TargetFramework net8.0-windows; UseWPF)

## Como utilizar

Navegação: menu lateral com três seções.

### Pessoas
- Filtros: por Nome e CPF; botão de Busca para aplicar
- Adicionar Pessoa: abre modal; preencha Nome e CPF e clique em Salvar
- Selecionar uma pessoa e usar “Incluir Pedido” levará até a aba Pedidos já com a pessoa pré-selecionada no modal de criação

### Produtos
- Filtros: Nome, Código, Valor mín/máx; aplique com o botão de Busca
- Adicionar Produto: define Nome, Código e Valor e salva

### Pedidos
- Cabeçalho:
	- Combo de Pessoa (filtro)
	- Limpar filtros
	- Filtros adicionais:
		- “Apenas entregues” (Status = Recebido)
		- “Apenas pagos” (Status = Pago)
		- “Pendentes de pagamento” (Status = Pendente)
	- Ações:
		- Novo Pedido: abre modal de criação
		- Finalizar Selecionado: finaliza o pedido selecionado mantendo o Status atual (não muda para Pago automaticamente)
		- Marcar como Pago / Enviado / Recebido: muda o Status do pedido selecionado (somente após finalizado)

- Modal “Novo Pedido”:
	- Pessoa: selecione ou já vem preenchida ao partir da aba Pessoas
	- Forma de Pagamento: valores do enum (Dinheiro, Cartao, Boleto)
	- Itens: escolha um Produto, informe a Quantidade e clique em Adicionar
	- O total é recalculado automaticamente conforme os itens
	- Criar Pedido: salva o pedido, fecha o modal e lista na grade

- Regras de negócio:
	- Finalizar não altera automaticamente o Status para Pago; mantém “Pendente” por padrão
	- Pedidos finalizados não podem ser editados
	- Para marcar como Pago/Enviado/Recebido é necessário que o pedido esteja finalizado

## Como testar (passo a passo)

Teste rápido end-to-end:
1. Inicie o app
2. Vá em Produtos e adicione ao menos um produto
3. Vá em Pessoas e adicione uma pessoa
4. Com a pessoa selecionada, clique em “Incluir Pedido”
5. No modal, confirme a pessoa, selecione a forma de pagamento
6. Adicione 1 ou mais itens (produto + quantidade)
7. Clique em “Criar Pedido”
8. Na aba Pedidos, selecione o pedido recém-criado e clique em “Finalizar Selecionado”
9. Com o pedido finalizado, use “Marcar como Pago” (ou Enviado/Recebido) e observe o Status mudar na grade
10. Experimente os filtros adicionais (Apenas pagos, Apenas entregues, Pendentes de pagamento)

Casos de validação:
- Tentar salvar pedido sem pessoa ou sem itens deve exibir aviso
- Tentar editar um pedido finalizado deve ser bloqueado
- Botões de Marcar como Pago/Enviado/Recebido devem ficar desabilitados enquanto o pedido não estiver finalizado

## Dúvidas comuns
- Onde ficam os dados? Em `Data/*.json` (copiados para a pasta de saída ao compilar/rodar)
- Posso resetar os dados? Feche o app e apague os JSONs da pasta `bin/Debug/net8.0-windows/Data` (ou `bin/Release/...`)