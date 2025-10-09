using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfApp.Infra;
using WpfApp.Models;
using WpfApp.Services;
using WpfApp.Services.Validators;

namespace WpfApp.ViewModels
{
    public class PessoasViewModel : INotifyPropertyChanged
    {
        private readonly PessoaRepository _repository;
        private readonly PedidoRepository _pedidoRepo;
        private readonly ProdutoRepository _produtoRepo;

        private ObservableCollection<Pessoa> _pessoas = new();
        private Pessoa? _pessoaSelecionada;
        private string _filtroNome = string.Empty;
        private string _filtroCpf = string.Empty;
        private bool _isModalAddPessoaVisible;
        private Pessoa _editBuffer = new();

        public ObservableCollection<Pessoa> Pessoas
        {
            get => _pessoas;
            set { _pessoas = value; OnPropertyChanged(); }
        }

        public Pessoa? PessoaSelecionada
        {
            get => _pessoaSelecionada;
            set
            {
                // Se trocar de pessoa enquanto inclui pedido, descarta o rascunho
                if (_pessoaSelecionada != value && IsIncluindoPedido)
                {
                    CancelarInclusaoPedido();
                }

                _pessoaSelecionada = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PodeIncluirPedido));
                CarregarPedidosDaPessoa();
            }
        }

        public string FiltroNome
        {
            get => _filtroNome;
            set { _filtroNome = value; OnPropertyChanged(); }
        }

        public string FiltroCpf
        {
            get => _filtroCpf;
            set { _filtroCpf = value; OnPropertyChanged(); }
        }

        public bool IsModalAddPessoaVisible
        {
            get => _isModalAddPessoaVisible;
            set { _isModalAddPessoaVisible = value; OnPropertyChanged(); }
        }

        public Pessoa EditBuffer
        {
            get => _editBuffer;
            set { _editBuffer = value; OnPropertyChanged(); }
        }

        // Comandos Pessoas
        public ICommand CarregarCommand { get; }
        public ICommand BuscarCommand { get; }
        public ICommand AbrirModalAddPessoaCommand { get; }
        public ICommand FecharModalAddPessoaCommand { get; }
        public ICommand SalvarPessoaModalCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; }
        // Aliases para PessoasView.xaml (inline)
        public ICommand IncluirCommand { get; }
        public ICommand SalvarCommand { get; }

        // ===== Pedidos vinculados à Pessoa =====
        public ObservableCollection<Pedido> PedidosDaPessoa { get; } = new();
        public ObservableCollection<Produto> Produtos { get; } = new();
        public ObservableCollection<PedidoItem> ItensPedidoEmEdicao { get; } = new();

        private bool _isIncluindoPedido;
        public bool IsIncluindoPedido
        {
            get => _isIncluindoPedido;
            set { _isIncluindoPedido = value; OnPropertyChanged(); OnPropertyChanged(nameof(PodeIncluirPedido)); }
        }

        // Habilita o botão "Incluir Pedido" quando há pessoa selecionada e não está incluindo
        public bool PodeIncluirPedido => PessoaSelecionada != null && !IsIncluindoPedido;

        private Produto? _produtoSelecionadoParaAdicionar;
        public Produto? ProdutoSelecionadoParaAdicionar
        {
            get => _produtoSelecionadoParaAdicionar;
            set { _produtoSelecionadoParaAdicionar = value; OnPropertyChanged(); }
        }

        private int _qtdeParaAdicionar = 1;
        public int QtdeParaAdicionar
        {
            get => _qtdeParaAdicionar;
            set { _qtdeParaAdicionar = value; OnPropertyChanged(); }
        }

        private PedidoItem? _itemSelecionadoEmEdicao;
        public PedidoItem? ItemSelecionadoEmEdicao
        {
            get => _itemSelecionadoEmEdicao;
            set { _itemSelecionadoEmEdicao = value; OnPropertyChanged(); }
        }

        private string _formaPagamentoSelecionada = "Dinheiro";
        public string FormaPagamentoSelecionada
        {
            get => _formaPagamentoSelecionada;
            set { _formaPagamentoSelecionada = value; OnPropertyChanged(); }
        }

        private decimal _totalPedidoEmEdicao;
        public decimal TotalPedidoEmEdicao
        {
            get => _totalPedidoEmEdicao;
            set { _totalPedidoEmEdicao = value; OnPropertyChanged(); }
        }

        // Filtros de pedidos
        private bool _mostrarApenasRecebidos;
        public bool MostrarApenasRecebidos
        {
            get => _mostrarApenasRecebidos;
            set { _mostrarApenasRecebidos = value; OnPropertyChanged(); }
        }

        private bool _mostrarApenasPagos;
        public bool MostrarApenasPagos
        {
            get => _mostrarApenasPagos;
            set { _mostrarApenasPagos = value; OnPropertyChanged(); }
        }

        private bool _mostrarApenasPendentes;
        public bool MostrarApenasPendentes
        {
            get => _mostrarApenasPendentes;
            set { _mostrarApenasPendentes = value; OnPropertyChanged(); }
        }

        // Comandos de pedidos
        public ICommand IncluirPedidoCommand { get; }
        public ICommand AdicionarItemPedidoCommand { get; }
        public ICommand RemoverItemPedidoCommand { get; }
        public ICommand FinalizarPedidoCommand { get; }
        public ICommand CancelarInclusaoPedidoCommand { get; }
        public ICommand MarcarPagoCommand { get; }
        public ICommand MarcarEnviadoCommand { get; }
        public ICommand MarcarRecebidoCommand { get; }
        public ICommand AplicarFiltrosPedidosCommand { get; }

        // Construtor padrão (para design-time ou standalone)
        public PessoasViewModel()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _repository = new PessoaRepository(Path.Combine(baseDir, "Data", "pessoas.json"));
            _pedidoRepo = new PedidoRepository(Path.Combine(baseDir, "Data", "pedidos.json"));
            _produtoRepo = new ProdutoRepository(Path.Combine(baseDir, "Data", "produtos.json"));

            // Comandos Pessoas
            CarregarCommand = new RelayCommand(_ => Carregar());
            BuscarCommand = new RelayCommand(_ => Buscar());
            AbrirModalAddPessoaCommand = new RelayCommand(_ => AbrirModalAddPessoa());
            FecharModalAddPessoaCommand = new RelayCommand(_ => FecharModalAddPessoa());
            SalvarPessoaModalCommand = new RelayCommand(_ => SalvarPessoaModal(), _ => CanSalvarPessoa());
            EditarCommand = new RelayCommand(p => { if (p is Pessoa pes) Editar(pes); });
            ExcluirCommand = new RelayCommand(p => { if (p is Pessoa pes) Excluir(pes); });
            // Aliases inline
            IncluirCommand = new RelayCommand(_ => AbrirModalAddPessoa());
            SalvarCommand = new RelayCommand(_ => SalvarPessoaModal(), _ => CanSalvarPessoa());

            // Comandos Pedidos
            IncluirPedidoCommand = new RelayCommand(_ => IncluirPedido());
            AdicionarItemPedidoCommand = new RelayCommand(_ => AdicionarItemPedido());
            RemoverItemPedidoCommand = new RelayCommand(_ => RemoverItemPedido());
            FinalizarPedidoCommand = new RelayCommand(_ => FinalizarPedido());
            CancelarInclusaoPedidoCommand = new RelayCommand(_ => CancelarInclusaoPedido());
            MarcarPagoCommand = new RelayCommand(p => MarcarStatus(p, StatusPedido.Pago));
            MarcarEnviadoCommand = new RelayCommand(p => MarcarStatus(p, StatusPedido.Enviado));
            MarcarRecebidoCommand = new RelayCommand(p => MarcarStatus(p, StatusPedido.Recebido));
            AplicarFiltrosPedidosCommand = new RelayCommand(_ => AplicarFiltrosPedidos());

            Carregar();
        }

        // Construtor usado pelo MainViewModel com repositórios injetados
        public PessoasViewModel(PessoaRepository pessoaRepo, ProdutoRepository? produtoRepo, PedidoRepository? pedidoRepo)
        {
            _repository = pessoaRepo;
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _pedidoRepo = pedidoRepo ?? new PedidoRepository(Path.Combine(baseDir, "Data", "pedidos.json"));
            _produtoRepo = produtoRepo ?? new ProdutoRepository(Path.Combine(baseDir, "Data", "produtos.json"));

            // Comandos Pessoas
            CarregarCommand = new RelayCommand(_ => Carregar());
            BuscarCommand = new RelayCommand(_ => Buscar());
            AbrirModalAddPessoaCommand = new RelayCommand(_ => AbrirModalAddPessoa());
            FecharModalAddPessoaCommand = new RelayCommand(_ => FecharModalAddPessoa());
            SalvarPessoaModalCommand = new RelayCommand(_ => SalvarPessoaModal(), _ => CanSalvarPessoa());
            EditarCommand = new RelayCommand(p => { if (p is Pessoa pes) Editar(pes); });
            ExcluirCommand = new RelayCommand(p => { if (p is Pessoa pes) Excluir(pes); });
            // Aliases inline
            IncluirCommand = new RelayCommand(_ => AbrirModalAddPessoa());
            SalvarCommand = new RelayCommand(_ => SalvarPessoaModal(), _ => CanSalvarPessoa());

            // Comandos Pedidos
            IncluirPedidoCommand = new RelayCommand(_ => IncluirPedido());
            AdicionarItemPedidoCommand = new RelayCommand(_ => AdicionarItemPedido());
            RemoverItemPedidoCommand = new RelayCommand(_ => RemoverItemPedido());
            FinalizarPedidoCommand = new RelayCommand(_ => FinalizarPedido());
            CancelarInclusaoPedidoCommand = new RelayCommand(_ => CancelarInclusaoPedido());
            MarcarPagoCommand = new RelayCommand(p => MarcarStatus(p, StatusPedido.Pago));
            MarcarEnviadoCommand = new RelayCommand(p => MarcarStatus(p, StatusPedido.Enviado));
            MarcarRecebidoCommand = new RelayCommand(p => MarcarStatus(p, StatusPedido.Recebido));
            AplicarFiltrosPedidosCommand = new RelayCommand(_ => AplicarFiltrosPedidos());

            Carregar();
        }

        private void Carregar()
        {
            try
            {
                var pessoas = _repository.GetAll();
                Pessoas.Clear();
                foreach (var p in pessoas)
                {
                    Pessoas.Add(p);
                }
                CarregarPedidosDaPessoa();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao carregar pessoas: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Buscar()
        {
            try
            {
                var resultado = _repository.Buscar(FiltroNome, FiltroCpf);

                Pessoas.Clear();
                foreach (var p in resultado)
                {
                    Pessoas.Add(p);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao buscar pessoas: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void AbrirModalAddPessoa()
        {
            EditBuffer = new Pessoa();
            IsModalAddPessoaVisible = true;
        }

        private void FecharModalAddPessoa()
        {
            IsModalAddPessoaVisible = false;
            EditBuffer = new Pessoa();
        }

        private bool CanSalvarPessoa()
        {
            if (EditBuffer == null) return false;
            if (string.IsNullOrWhiteSpace(EditBuffer.Nome)) return false;
            if (string.IsNullOrWhiteSpace(EditBuffer.Cpf)) return false;
            return CpfValidator.IsValid(EditBuffer.Cpf);
        }

        private void SalvarPessoaModal()
        {
            try
            {
                if (!CanSalvarPessoa())
                {
                    MessageBox.Show(
                        "Preencha corretamente Nome e CPF válido.",
                        "Validação",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (EditBuffer.Id == 0)
                {
                    _repository.Add(EditBuffer);
                }
                else
                {
                    _repository.Update(EditBuffer);
                }

                Carregar();
                FecharModalAddPessoa();

                MessageBox.Show(
                    "Pessoa salva com sucesso!",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao salvar pessoa: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Editar(Pessoa pessoa)
        {
            if (pessoa == null) return;

            EditBuffer = new Pessoa
            {
                Id = pessoa.Id,
                Nome = pessoa.Nome,
                Cpf = pessoa.Cpf,
                Endereco = pessoa.Endereco
            };

            IsModalAddPessoaVisible = true;
        }

        private void Excluir(Pessoa pessoa)
        {
            if (pessoa == null) return;

            var result = MessageBox.Show(
                $"Deseja realmente excluir {pessoa.Nome}?",
                "Confirmar Exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _repository.Delete(pessoa.Id);
                    Carregar();

                    MessageBox.Show(
                        "Pessoa excluída com sucesso!",
                        "Sucesso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erro ao excluir pessoa: {ex.Message}",
                        "Erro",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        // ====== Pedidos: funcionalidades ======
        private void CarregarPedidosDaPessoa()
        {
            PedidosDaPessoa.Clear();
            if (PessoaSelecionada == null) return;
            foreach (var p in _pedidoRepo.BuscarPorPessoa(PessoaSelecionada.Id))
                PedidosDaPessoa.Add(p);
        }

        private void IncluirPedido()
        {
            if (PessoaSelecionada == null)
            {
                MessageBox.Show("Selecione uma pessoa antes.");
                return;
            }

            Produtos.Clear();
            foreach (var pr in _produtoRepo.GetAll()) Produtos.Add(pr);

            ItensPedidoEmEdicao.Clear();
            QtdeParaAdicionar = 1;
            ProdutoSelecionadoParaAdicionar = null;
            FormaPagamentoSelecionada = "Dinheiro";
            TotalPedidoEmEdicao = 0m;
            IsIncluindoPedido = true;
        }

        private void AdicionarItemPedido()
        {
            if (!IsIncluindoPedido || ProdutoSelecionadoParaAdicionar == null || QtdeParaAdicionar <= 0) return;
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

            QtdeParaAdicionar = 1;
            RecalcularTotalPedido();
        }

        private void RemoverItemPedido()
        {
            if (ItemSelecionadoEmEdicao == null) return;
            ItensPedidoEmEdicao.Remove(ItemSelecionadoEmEdicao);
            RecalcularTotalPedido();
        }

        private void RecalcularTotalPedido()
        {
            TotalPedidoEmEdicao = ItensPedidoEmEdicao.Sum(i => i.TotalItem);
        }

        private void FinalizarPedido()
        {
            if (!IsIncluindoPedido) return;
            if (PessoaSelecionada == null)
            {
                MessageBox.Show("Selecione uma pessoa.");
                return;
            }
            if (ItensPedidoEmEdicao.Count == 0)
            {
                MessageBox.Show("Adicione pelo menos um item.");
                return;
            }

            var forma = FormaPagamento.Dinheiro;
            if (Enum.TryParse<FormaPagamento>(FormaPagamentoSelecionada, out var fp))
                forma = fp;

            var pedido = new Pedido
            {
                PessoaId = PessoaSelecionada.Id,
                PessoaNome = PessoaSelecionada.Nome,
                DataVenda = DateTime.Now,
                Status = StatusPedido.Pendente,
                FormaPagamento = forma,
                IsFinalizado = true,
                Itens = ItensPedidoEmEdicao.Select(i => new PedidoItem
                {
                    ProdutoId = i.ProdutoId,
                    ProdutoNome = i.ProdutoNome,
                    Quantidade = i.Quantidade,
                    ValorUnitario = i.ValorUnitario
                }).ToList()
            };
            pedido.ValorTotal = pedido.Itens.Sum(i => i.TotalItem);

            var salvo = _pedidoRepo.Add(pedido);
            PedidosDaPessoa.Insert(0, salvo);

            CancelarInclusaoPedido();
        }

        private void CancelarInclusaoPedido()
        {
            IsIncluindoPedido = false;
            ItensPedidoEmEdicao.Clear();
            ProdutoSelecionadoParaAdicionar = null;
            QtdeParaAdicionar = 1;
            TotalPedidoEmEdicao = 0m;
        }

        private void MarcarStatus(object? param, StatusPedido status)
        {
            if (param is not Pedido ped) return;
            if (ped.IsFinalizado)
            {
                MessageBox.Show("Pedido finalizado não pode ter o status alterado.");
                return;
            }
            ped.Status = status;
            _pedidoRepo.Update(ped);
            var idx = PedidosDaPessoa.ToList().FindIndex(p => p.Id == ped.Id);
            if (idx >= 0) PedidosDaPessoa[idx] = ped;
        }

        private void AplicarFiltrosPedidos()
        {
            if (PessoaSelecionada == null)
            {
                PedidosDaPessoa.Clear();
                return;
            }

            var todos = _pedidoRepo.BuscarPorPessoa(PessoaSelecionada.Id).ToList();
            var filtrado = todos.AsEnumerable();

            var usarRecebidos = MostrarApenasRecebidos;
            var usarPagos = MostrarApenasPagos;
            var usarPendentes = MostrarApenasPendentes;

            if (usarRecebidos || usarPagos || usarPendentes)
            {
                filtrado = filtrado.Where(p =>
                    (usarRecebidos && p.Status == StatusPedido.Recebido) ||
                    (usarPagos && p.Status == StatusPedido.Pago) ||
                    (usarPendentes && p.Status == StatusPedido.Pendente));
            }

            PedidosDaPessoa.Clear();
            foreach (var p in filtrado.OrderByDescending(p => p.DataVenda ?? DateTime.MinValue))
                PedidosDaPessoa.Add(p);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
