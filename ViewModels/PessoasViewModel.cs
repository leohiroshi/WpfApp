using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WpfApp.Infra;
using WpfApp.Models;
using WpfApp.Services;
using WpfApp.Services.Validators;

namespace WpfApp.ViewModels
{
    public class PessoasViewModel : BaseViewModel
    {
        private readonly PessoaRepository _pessoasRepo;
        private readonly ProdutoRepository _produtosRepo;
        private readonly PedidoRepository _pedidosRepo;

        public ObservableCollection<Pessoa> Pessoas { get; } = new();
        private Pessoa _pessoaSelecionada;
        public Pessoa PessoaSelecionada
        {
            get => _pessoaSelecionada;
            set
            {
                if (SetProperty(ref _pessoaSelecionada, value))
                {
                    RecarregarPedidosDaPessoa();
                    IncluirPedidoCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // Filtros Pessoa
        private string _filtroNome = string.Empty;
        public string FiltroNome
        {
            get => _filtroNome;
            set => SetProperty(ref _filtroNome, value);
        }

        private string _filtroCpf = string.Empty;
        public string FiltroCpf
        {
            get => _filtroCpf;
            set => SetProperty(ref _filtroCpf, value);
        }

        // Edição Pessoa
        private Pessoa _editBuffer = new();
        public Pessoa EditBuffer
        {
            get => _editBuffer;
            set => SetProperty(ref _editBuffer, value);
        }

        // Estado Pessoa
        private bool _isEditando;
        public bool IsEditando
        {
            get => _isEditando;
            set
            {
                if (SetProperty(ref _isEditando, value))
                {
                    IncluirCommand.RaiseCanExecuteChanged();
                    EditarCommand.RaiseCanExecuteChanged();
                    SalvarCommand.RaiseCanExecuteChanged();
                    ExcluirCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // Dados de Produtos (para incluir pedido)
        public ObservableCollection<Produto> Produtos { get; } = new();

        // Inclusão de Pedido (inline)
        private bool _isIncluindoPedido;
        public bool IsIncluindoPedido
        {
            get => _isIncluindoPedido;
            set
            {
                if (SetProperty(ref _isIncluindoPedido, value))
                {
                    IncluirPedidoCommand.RaiseCanExecuteChanged();
                    AdicionarItemPedidoCommand.RaiseCanExecuteChanged();
                    RemoverItemPedidoCommand.RaiseCanExecuteChanged();
                    FinalizarPedidoCommand.RaiseCanExecuteChanged();
                    CancelarInclusaoPedidoCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<PedidoItem> ItensPedidoEmEdicao { get; } = new();

        private Produto _produtoSelecionadoParaAdicionar;
        public Produto ProdutoSelecionadoParaAdicionar
        {
            get => _produtoSelecionadoParaAdicionar;
            set
            {
                if (SetProperty(ref _produtoSelecionadoParaAdicionar, value))
                    AdicionarItemPedidoCommand.RaiseCanExecuteChanged();
            }
        }

        private int _qtdeParaAdicionar = 1;
        public int QtdeParaAdicionar
        {
            get => _qtdeParaAdicionar;
            set
            {
                if (SetProperty(ref _qtdeParaAdicionar, value))
                    AdicionarItemPedidoCommand.RaiseCanExecuteChanged();
            }
        }

        private PedidoItem _itemSelecionadoEmEdicao;
        public PedidoItem ItemSelecionadoEmEdicao
        {
            get => _itemSelecionadoEmEdicao;
            set
            {
                if (SetProperty(ref _itemSelecionadoEmEdicao, value))
                    RemoverItemPedidoCommand.RaiseCanExecuteChanged();
            }
        }

        private FormaPagamento _formaPagamentoSelecionada = FormaPagamento.Dinheiro;
        public FormaPagamento FormaPagamentoSelecionada
        {
            get => _formaPagamentoSelecionada;
            set => SetProperty(ref _formaPagamentoSelecionada, value);
        }

        private decimal _totalPedidoEmEdicao;
        public decimal TotalPedidoEmEdicao
        {
            get => _totalPedidoEmEdicao;
            private set => SetProperty(ref _totalPedidoEmEdicao, value);
        }

        // Pedidos da Pessoa
        public ObservableCollection<Pedido> PedidosDaPessoa { get; } = new();

        // Filtros rápidos de pedidos
        private bool _mostrarApenasRecebidos;
        public bool MostrarApenasRecebidos
        {
            get => _mostrarApenasRecebidos;
            set => SetProperty(ref _mostrarApenasRecebidos, value);
        }

        private bool _mostrarApenasPagos;
        public bool MostrarApenasPagos
        {
            get => _mostrarApenasPagos;
            set => SetProperty(ref _mostrarApenasPagos, value);
        }

        private bool _mostrarApenasPendentes;
        public bool MostrarApenasPendentes
        {
            get => _mostrarApenasPendentes;
            set => SetProperty(ref _mostrarApenasPendentes, value);
        }

        // Comandos gerais Pessoas
        public RelayCommand CarregarCommand { get; }
        public RelayCommand BuscarCommand { get; }
        public RelayCommand IncluirCommand { get; }
        public RelayCommand EditarCommand { get; }
        public RelayCommand SalvarCommand { get; }
        public RelayCommand ExcluirCommand { get; }
        public RelayCommand IncluirPessoaTesteCommand { get; }

        // Comandos de pedido inline
        public RelayCommand IncluirPedidoCommand { get; }
        public RelayCommand AdicionarItemPedidoCommand { get; }
        public RelayCommand RemoverItemPedidoCommand { get; }
        public RelayCommand FinalizarPedidoCommand { get; }
        public RelayCommand CancelarInclusaoPedidoCommand { get; }

        // Comandos por linha na grid de pedidos (não genéricos)
        public RelayCommand MarcarPagoCommand { get; }
        public RelayCommand MarcarEnviadoCommand { get; }
        public RelayCommand MarcarRecebidoCommand { get; }

        // Aplicar filtros rápidos
        public RelayCommand AplicarFiltrosPedidosCommand { get; }

        public PessoasViewModel(PessoaRepository pessoasRepo, ProdutoRepository produtosRepo, PedidoRepository pedidosRepo)
        {
            _pessoasRepo = pessoasRepo;
            _produtosRepo = produtosRepo;
            _pedidosRepo = pedidosRepo;

            CarregarCommand = new RelayCommand(CarregarPessoas);
            BuscarCommand = new RelayCommand(Buscar);
            IncluirCommand = new RelayCommand(Incluir, () => !IsEditando);
            EditarCommand = new RelayCommand(Editar, () => PessoaSelecionada != null && !IsEditando);
            SalvarCommand = new RelayCommand(Salvar, () => IsEditando);
            ExcluirCommand = new RelayCommand(Excluir, () => PessoaSelecionada != null && !IsEditando);

            // Pedidos inline
            IncluirPedidoCommand = new RelayCommand(IniciarInclusaoPedido, () => PessoaSelecionada != null && !IsIncluindoPedido);
            AdicionarItemPedidoCommand = new RelayCommand(AdicionarItemPedido, () => IsIncluindoPedido && ProdutoSelecionadoParaAdicionar != null && QtdeParaAdicionar > 0);
            RemoverItemPedidoCommand = new RelayCommand(RemoverItemPedido, () => IsIncluindoPedido && ItemSelecionadoEmEdicao != null);
            FinalizarPedidoCommand = new RelayCommand(FinalizarPedido, () => IsIncluindoPedido && ItensPedidoEmEdicao.Count > 0);
            CancelarInclusaoPedidoCommand = new RelayCommand(CancelarInclusaoPedido, () => IsIncluindoPedido);

            // Ações por linha (usando RelayCommand não genérico com parâmetro object)
            MarcarPagoCommand = new RelayCommand(
                param => MarcarPago(param as Pedido),
                param => param is Pedido ped && ped.IsFinalizado && ped.Status != StatusPedido.Pago
            );
            MarcarEnviadoCommand = new RelayCommand(
                param => MarcarEnviado(param as Pedido),
                param => param is Pedido ped && ped.IsFinalizado
            );
            MarcarRecebidoCommand = new RelayCommand(
                param => MarcarRecebido(param as Pedido),
                param => param is Pedido ped && ped.IsFinalizado
            );

            AplicarFiltrosPedidosCommand = new RelayCommand(RecarregarPedidosDaPessoa);

            CarregarPessoas();
            CarregarProdutos();
        }

        private void CarregarPessoas()
        {
            Pessoas.Clear();
            foreach (var p in _pessoasRepo.GetAll())
                Pessoas.Add(p);
        }

        private void CarregarProdutos()
        {
            Produtos.Clear();
            foreach (var pr in _produtosRepo.GetAll())
                Produtos.Add(pr);
        }

        private void Buscar()
        {
            Pessoas.Clear();
            foreach (var p in _pessoasRepo.Buscar(FiltroNome, FiltroCpf))
                Pessoas.Add(p);
        }

        private void Incluir()
        {
            EditBuffer = new Pessoa();
            IsEditando = true;
        }

        private void Editar()
        {
            if (PessoaSelecionada == null) return;
            EditBuffer = new Pessoa
            {
                Id = PessoaSelecionada.Id,
                Nome = PessoaSelecionada.Nome,
                Cpf = PessoaSelecionada.Cpf,
                Endereco = PessoaSelecionada.Endereco
            };
            IsEditando = true;
        }

        private void Salvar()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditBuffer.Nome))
                {
                    MessageBox.Show("Nome é obrigatório.");
                    return;
                }
                if (!CpfValidator.IsValid(EditBuffer.Cpf))
                {
                    MessageBox.Show("CPF inválido.");
                    return;
                }

                if (EditBuffer.Id == 0)
                {
                    var adicionada = _pessoasRepo.Add(EditBuffer);
                    Pessoas.Add(adicionada);
                    PessoaSelecionada = adicionada;
                }
                else
                {
                    _pessoasRepo.Update(EditBuffer);
                    var idx = Pessoas.ToList().FindIndex(p => p.Id == EditBuffer.Id);
                    if (idx >= 0)
                    {
                        Pessoas[idx] = EditBuffer;
                        PessoaSelecionada = EditBuffer;
                    }
                }

                IsEditando = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar: {ex.Message}");
            }
        }

        private void Excluir()
        {
            if (PessoaSelecionada == null) return;
            if (MessageBox.Show($"Excluir {PessoaSelecionada.Nome}?", "Confirmação",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            if (_pessoasRepo.Delete(PessoaSelecionada.Id))
            {
                Pessoas.Remove(PessoaSelecionada);
                PessoaSelecionada = null;
                PedidosDaPessoa.Clear();
            }
        }

        // Inclusão de Pedido inline
        private void IniciarInclusaoPedido()
        {
            if (PessoaSelecionada == null)
            {
                MessageBox.Show("Selecione uma pessoa para incluir o pedido.");
                return;
            }

            ItensPedidoEmEdicao.Clear();
            ProdutoSelecionadoParaAdicionar = null;
            QtdeParaAdicionar = 1;
            FormaPagamentoSelecionada = FormaPagamento.Dinheiro;
            TotalPedidoEmEdicao = 0m;

            IsIncluindoPedido = true;
        }

        private void AdicionarItemPedido()
        {
            if (ProdutoSelecionadoParaAdicionar == null || QtdeParaAdicionar <= 0) return;

            var existente = ItensPedidoEmEdicao.FirstOrDefault(i => i.ProdutoId == ProdutoSelecionadoParaAdicionar.Id);
            if (existente != null)
            {
                existente.Quantidade += QtdeParaAdicionar;
            }
            else
            {
                ItensPedidoEmEdicao.Add(new PedidoItem
                {
                    ProdutoId = ProdutoSelecionadoParaAdicionar.Id,
                    ProdutoNome = ProdutoSelecionadoParaAdicionar.Nome,
                    Quantidade = QtdeParaAdicionar,
                    ValorUnitario = ProdutoSelecionadoParaAdicionar.Valor
                });
            }
            RecalcularTotalPedidoEmEdicao();
            QtdeParaAdicionar = 1;
        }

        private void RemoverItemPedido()
        {
            if (ItemSelecionadoEmEdicao == null) return;
            ItensPedidoEmEdicao.Remove(ItemSelecionadoEmEdicao);
            RecalcularTotalPedidoEmEdicao();
        }

        private void RecalcularTotalPedidoEmEdicao()
        {
            TotalPedidoEmEdicao = ItensPedidoEmEdicao.Sum(i => i.TotalItem);
        }

        private void FinalizarPedido()
        {
            try
            {
                if (PessoaSelecionada == null)
                {
                    MessageBox.Show("Selecione uma pessoa.");
                    return;
                }
                if (!ItensPedidoEmEdicao.Any())
                {
                    MessageBox.Show("Adicione ao menos um item.");
                    return;
                }

                var pedido = new Pedido
                {
                    PessoaId = PessoaSelecionada.Id,
                    PessoaNome = PessoaSelecionada.Nome,
                    Itens = ItensPedidoEmEdicao.ToList(),
                    ValorTotal = ItensPedidoEmEdicao.Sum(i => i.TotalItem),
                    DataVenda = DateTime.Now,
                    FormaPagamento = FormaPagamentoSelecionada,
                    Status = StatusPedido.Pendente,
                    IsFinalizado = true
                };

                var salvo = _pedidosRepo.Add(pedido);

                // Atualiza a lista da pessoa
                PedidosDaPessoa.Insert(0, salvo);

                // Sai do modo de inclusão
                IsIncluindoPedido = false;

                MessageBox.Show("Pedido finalizado e salvo com sucesso.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao finalizar pedido: {ex.Message}");
            }
        }

        private void CancelarInclusaoPedido()
        {
            // Descarta o rascunho (nada foi salvo)
            IsIncluindoPedido = false;
            ItensPedidoEmEdicao.Clear();
            TotalPedidoEmEdicao = 0m;
        }

        // Pedidos da Pessoa
        private void RecarregarPedidosDaPessoa()
        {
            PedidosDaPessoa.Clear();
            if (PessoaSelecionada == null)
                return;

            var pedidos = _pedidosRepo.BuscarPorPessoa(PessoaSelecionada.Id);

            // Aplicar filtros rápidos
            if (MostrarApenasRecebidos)
                pedidos = pedidos.Where(p => p.Status == StatusPedido.Recebido);
            if (MostrarApenasPagos)
                pedidos = pedidos.Where(p => p.Status == StatusPedido.Pago);
            if (MostrarApenasPendentes)
                pedidos = pedidos.Where(p => p.Status == StatusPedido.Pendente);

            foreach (var ped in pedidos)
                PedidosDaPessoa.Add(ped);
        }

        // Ações por linha
        private void MarcarPago(Pedido ped)
        {
            if (ped == null) return;
            if (!ped.IsFinalizado)
            {
                MessageBox.Show("Finalize o pedido antes de marcar como Pago.");
                return;
            }

            ped.Status = StatusPedido.Pago;
            _pedidosRepo.Update(ped);
            AtualizarPedidoNaColecao(ped);
        }

        private void MarcarEnviado(Pedido ped)
        {
            if (ped == null) return;
            if (!ped.IsFinalizado)
            {
                MessageBox.Show("Finalize o pedido antes de marcar como Enviado.");
                return;
            }

            ped.Status = StatusPedido.Enviado;
            _pedidosRepo.Update(ped);
            AtualizarPedidoNaColecao(ped);
        }

        private void MarcarRecebido(Pedido ped)
        {
            if (ped == null) return;
            if (!ped.IsFinalizado)
            {
                MessageBox.Show("Finalize o pedido antes de marcar como Recebido.");
                return;
            }

            ped.Status = StatusPedido.Recebido;
            _pedidosRepo.Update(ped);
            AtualizarPedidoNaColecao(ped);
        }

        private void AtualizarPedidoNaColecao(Pedido ped)
        {
            var idx = PedidosDaPessoa.ToList().FindIndex(x => x.Id == ped.Id);
            if (idx >= 0)
                PedidosDaPessoa[idx] = ped;
        }
    }
}