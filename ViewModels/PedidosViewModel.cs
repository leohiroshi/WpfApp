using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WpfApp.Infra;
using WpfApp.Models;
using WpfApp.Services;

namespace WpfApp.ViewModels
{
    public class PedidosViewModel : BaseViewModel
    {
        private readonly PedidoRepository _pedidosRepo;
        public PessoaRepository _pessoasRepo;
        private readonly ProdutoRepository _produtosRepo;
        private readonly PedidoService _pedidoService;

        // Lista de valores do enum FormaPagamento para bindar no ComboBox
        public Array FormaPagamentoValues { get; } = Enum.GetValues(typeof(FormaPagamento));

        public ObservableCollection<Pedido> Pedidos { get; } = new();
        public ObservableCollection<Pessoa> Pessoas { get; } = new();
        public ObservableCollection<Produto> Produtos { get; } = new();
        public ObservableCollection<PedidoItem> ItensEdit { get; } = new();

        // Pessoa selecionada (filtro no header)
        private Pessoa? _pessoaSelecionada;
        public Pessoa? PessoaSelecionada
        {
            get => _pessoaSelecionada;
            set
            {
                if (SetProperty(ref _pessoaSelecionada, value))
                {
                    AplicarFiltroPessoaSelecionada();
                }
            }
        }

        // Pessoa selecionada (binding de ComboBox em criação/edição)
        private Pessoa? _pessoaSelecionadaEdicao;
        public Pessoa? PessoaSelecionadaEdicao
        {
            get => _pessoaSelecionadaEdicao;
            set
            {
                if (SetProperty(ref _pessoaSelecionadaEdicao, value) && value != null)
                {
                    EditBuffer.PessoaId = value.Id;
                    EditBuffer.PessoaNome = value.Nome;
                }
            }
        }

        private Pedido? _pedidoSelecionado;
        public Pedido? PedidoSelecionado
        {
            get => _pedidoSelecionado;
            set
            {
                if (SetProperty(ref _pedidoSelecionado, value))
                {
                    if (value != null)
                    {
                        EditBuffer = new Pedido
                        {
                            Id = value.Id,
                            PessoaId = value.PessoaId,
                            PessoaNome = value.PessoaNome,
                            DataVenda = value.DataVenda,
                            Status = value.Status,
                            ValorTotal = value.ValorTotal,
                            FormaPagamento = value.FormaPagamento,
                            IsFinalizado = value.IsFinalizado,
                            Itens = value.Itens?.Select(i => new PedidoItem
                            {
                                ProdutoId = i.ProdutoId,
                                ProdutoNome = i.ProdutoNome,
                                Quantidade = i.Quantidade,
                                ValorUnitario = i.ValorUnitario
                            }).ToList() ?? new()
                        };

                        ItensEdit.Clear();
                        foreach (var it in EditBuffer.Itens)
                            ItensEdit.Add(it);

                        PessoaSelecionadaEdicao = Pessoas.FirstOrDefault(p => p.Id == EditBuffer.PessoaId);
                        RecalcularTotal();
                        // Update CanExecutes for status commands
                        MarcarPagoCommand?.RaiseCanExecuteChanged();
                        MarcarEnviadoCommand?.RaiseCanExecuteChanged();
                        MarcarRecebidoCommand?.RaiseCanExecuteChanged();
                        FinalizarSelecionadoCommand?.RaiseCanExecuteChanged();
                    }
                    else
                    {
                        ItensEdit.Clear();
                        PessoaSelecionadaEdicao = null;
                        EditBuffer = new Pedido();
                        MarcarPagoCommand?.RaiseCanExecuteChanged();
                        MarcarEnviadoCommand?.RaiseCanExecuteChanged();
                        MarcarRecebidoCommand?.RaiseCanExecuteChanged();
                        FinalizarSelecionadoCommand?.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        // Filtros
        private string _filtroPessoa = string.Empty;
        public string FiltroPessoa
        {
            get => _filtroPessoa;
            set => SetProperty(ref _filtroPessoa, value);
        }

        private StatusPedido? _filtroStatus;
        public StatusPedido? FiltroStatus
        {
            get => _filtroStatus;
            set => SetProperty(ref _filtroStatus, value);
        }

        private DateTime? _filtroDataIni;
        public DateTime? FiltroDataIni
        {
            get => _filtroDataIni;
            set => SetProperty(ref _filtroDataIni, value);
        }

        private DateTime? _filtroDataFim;
        public DateTime? FiltroDataFim
        {
            get => _filtroDataFim;
            set => SetProperty(ref _filtroDataFim, value);
        }

        // Filtros adicionais exclusivos
        private bool _mostrarApenasEntregues;
        public bool MostrarApenasEntregues
        {
            get => _mostrarApenasEntregues;
            set
            {
                if (SetProperty(ref _mostrarApenasEntregues, value))
                {
                    if (value)
                    {
                        _mostrarApenasPagos = false; RaisePropertyChanged(nameof(MostrarApenasPagos));
                        _mostrarApenasPendentesPagamento = false; RaisePropertyChanged(nameof(MostrarApenasPendentesPagamento));
                    }
                    Buscar();
                }
            }
        }

        private bool _mostrarApenasPagos;
        public bool MostrarApenasPagos
        {
            get => _mostrarApenasPagos;
            set
            {
                if (SetProperty(ref _mostrarApenasPagos, value))
                {
                    if (value)
                    {
                        _mostrarApenasEntregues = false; RaisePropertyChanged(nameof(MostrarApenasEntregues));
                        _mostrarApenasPendentesPagamento = false; RaisePropertyChanged(nameof(MostrarApenasPendentesPagamento));
                    }
                    Buscar();
                }
            }
        }

        private bool _mostrarApenasPendentesPagamento;
        public bool MostrarApenasPendentesPagamento
        {
            get => _mostrarApenasPendentesPagamento;
            set
            {
                if (SetProperty(ref _mostrarApenasPendentesPagamento, value))
                {
                    if (value)
                    {
                        _mostrarApenasEntregues = false; RaisePropertyChanged(nameof(MostrarApenasEntregues));
                        _mostrarApenasPagos = false; RaisePropertyChanged(nameof(MostrarApenasPagos));
                    }
                    Buscar();
                }
            }
        }

        // Edição
        private Pedido _editBuffer = new();
        public Pedido EditBuffer
        {
            get => _editBuffer;
            set => SetProperty(ref _editBuffer, value);
        }

        private bool _isModalPedidoVisible;
        public bool IsModalPedidoVisible
        {
            get => _isModalPedidoVisible;
            set => SetProperty(ref _isModalPedidoVisible, value);
        }

        private Produto? _produtoSelecionadoParaAdicionar;
        public Produto? ProdutoSelecionadoParaAdicionar
        {
            get => _produtoSelecionadoParaAdicionar;
            set => SetProperty(ref _produtoSelecionadoParaAdicionar, value);
        }

        private int _qtdeParaAdicionar = 1;
        public int QtdeParaAdicionar
        {
            get => _qtdeParaAdicionar;
            set => SetProperty(ref _qtdeParaAdicionar, value);
        }

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
                    FinalizarCommand.RaiseCanExecuteChanged();
                    AdicionarItemCommand.RaiseCanExecuteChanged();
                    RemoverItemCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // Comandos
        public RelayCommand CarregarCommand { get; }
        public RelayCommand BuscarCommand { get; }
        public RelayCommand IncluirCommand { get; }
        public RelayCommand EditarCommand { get; }
        public RelayCommand SalvarCommand { get; }
        public RelayCommand ExcluirCommand { get; }
        public RelayCommand FinalizarCommand { get; }
        public RelayCommand FinalizarSelecionadoCommand { get; }
        public RelayCommand MarcarPagoCommand { get; }
        public RelayCommand MarcarEnviadoCommand { get; }
        public RelayCommand MarcarRecebidoCommand { get; }
        public RelayCommand AdicionarItemCommand { get; }
        public RelayCommand RemoverItemCommand { get; }
        public RelayCommand AbrirModalPedidoCommand { get; }
        public RelayCommand FecharModalPedidoCommand { get; }
        public RelayCommand LimparFiltrosCommand { get; }

        public PedidosViewModel(PedidoRepository pedidosRepo,
                                PessoaRepository pessoasRepo,
                                ProdutoRepository produtosRepo,
                                PedidoService pedidoService)
        {
            _pedidosRepo = pedidosRepo;
            _pessoasRepo = pessoasRepo;
            _produtosRepo = produtosRepo;
            _pedidoService = pedidoService;

            // Inicializações de seleção nulas
            _pessoaSelecionada = null;
            _pedidoSelecionado = null;
            _pessoaSelecionadaEdicao = null;
            _produtoSelecionadoParaAdicionar = null;
            _itemSelecionado = null;

            CarregarCommand = new RelayCommand(Carregar);
            BuscarCommand = new RelayCommand(Buscar);
            IncluirCommand = new RelayCommand(Incluir, () => !IsEditando);
            EditarCommand = new RelayCommand(Editar, () => PedidoSelecionado != null && !IsEditando && (PedidoSelecionado?.IsFinalizado != true));
            SalvarCommand = new RelayCommand(Salvar, () => IsEditando && !EditBuffer.IsFinalizado && ItensEdit.Count > 0);
            ExcluirCommand = new RelayCommand(Excluir, () => PedidoSelecionado != null && !IsEditando);
            FinalizarCommand = new RelayCommand(Finalizar, () => IsEditando && ItensEdit.Count > 0);
            FinalizarSelecionadoCommand = new RelayCommand(FinalizarSelecionado, () => PedidoSelecionado != null && PedidoSelecionado.IsFinalizado == false);
            MarcarPagoCommand = new RelayCommand(() => MarcarStatus(StatusPedido.Pago), () => PedidoSelecionado != null && PedidoSelecionado.IsFinalizado && PedidoSelecionado.Status != StatusPedido.Pago);
            MarcarEnviadoCommand = new RelayCommand(() => MarcarStatus(StatusPedido.Enviado), () => PedidoSelecionado != null && PedidoSelecionado.IsFinalizado && PedidoSelecionado.Status != StatusPedido.Enviado);
            MarcarRecebidoCommand = new RelayCommand(() => MarcarStatus(StatusPedido.Recebido), () => PedidoSelecionado != null && PedidoSelecionado.IsFinalizado && PedidoSelecionado.Status != StatusPedido.Recebido);
            AdicionarItemCommand = new RelayCommand(AdicionarItem, () => IsEditando && ProdutoSelecionadoParaAdicionar != null && QtdeParaAdicionar > 0);
            RemoverItemCommand = new RelayCommand(RemoverItem, () => IsEditando && ItemSelecionado != null);
            AbrirModalPedidoCommand = new RelayCommand(() => { Incluir(); IsModalPedidoVisible = true; });
            FecharModalPedidoCommand = new RelayCommand(() => { IsEditando = false; IsModalPedidoVisible = false; ItensEdit.Clear(); EditBuffer = new Pedido(); });
            LimparFiltrosCommand = new RelayCommand(LimparFiltros);

            Carregar();
        }

        private void LimparFiltros()
        {
            FiltroPessoa = string.Empty;
            FiltroStatus = null;
            FiltroDataIni = null;
            FiltroDataFim = null;
            PessoaSelecionada = null;
            MostrarApenasEntregues = false;
            MostrarApenasPagos = false;
            MostrarApenasPendentesPagamento = false;
            Buscar();
        }

        private void RecalcularTotal()
        {
            EditBuffer.ValorTotal = ItensEdit.Sum(i => i.TotalItem);
        }

        private void Carregar()
        {
            Pessoas.Clear();
            foreach (var p in _pessoasRepo.GetAll()) Pessoas.Add(p);

            Produtos.Clear();
            foreach (var pr in _produtosRepo.GetAll()) Produtos.Add(pr);

            Pedidos.Clear();
            foreach (var ped in _pedidosRepo.GetAll().OrderByDescending(p => p.DataVenda ?? DateTime.MinValue))
                Pedidos.Add(ped);
        }

        private void AplicarFiltroPessoaSelecionada()
        {
            var query = _pedidosRepo.GetAll().AsQueryable();
            if (PessoaSelecionada != null)
                query = query.Where(p => p.PessoaId == PessoaSelecionada.Id);

            Pedidos.Clear();
            foreach (var ped in query.OrderByDescending(p => p.DataVenda ?? DateTime.MinValue))
                Pedidos.Add(ped);
        }

        private void Buscar()
        {
            var query = _pedidosRepo.GetAll().AsQueryable();

            if (PessoaSelecionada != null)
                query = query.Where(p => p.PessoaId == PessoaSelecionada.Id);

            if (!string.IsNullOrWhiteSpace(FiltroPessoa))
                query = query.Where(p => (p.PessoaNome ?? string.Empty).Contains(FiltroPessoa, StringComparison.OrdinalIgnoreCase));

            if (FiltroStatus.HasValue)
                query = query.Where(p => p.Status == FiltroStatus.Value);

            if (FiltroDataIni.HasValue)
            {
                var dataIni = FiltroDataIni.Value.Date;
                query = query.Where(p => (p.DataVenda ?? DateTime.MinValue).Date >= dataIni);
            }

            if (FiltroDataFim.HasValue)
            {
                var dataFim = FiltroDataFim.Value.Date;
                query = query.Where(p => (p.DataVenda ?? DateTime.MinValue).Date <= dataFim);
            }

            if (MostrarApenasEntregues)
                query = query.Where(p => p.Status == StatusPedido.Recebido);

            if (MostrarApenasPagos)
                query = query.Where(p => p.Status == StatusPedido.Pago);

            if (MostrarApenasPendentesPagamento)
                query = query.Where(p => p.Status == StatusPedido.Pendente);

            Pedidos.Clear();
            foreach (var ped in query.OrderByDescending(p => p.DataVenda ?? DateTime.MinValue))
                Pedidos.Add(ped);
        }

        private void Incluir()
        {
            EditBuffer = new Pedido
            {
                Status = StatusPedido.Pendente,
                Itens = new System.Collections.Generic.List<PedidoItem>(),
                ValorTotal = 0m
            };
            ItensEdit.Clear();
            PessoaSelecionadaEdicao = null;
            QtdeParaAdicionar = 1;
            ProdutoSelecionadoParaAdicionar = null;
            IsEditando = true;
        }

        private void Editar()
        {
            if (PedidoSelecionado == null) return;

            // Bloquear edição de pedidos finalizados
            if (PedidoSelecionado.IsFinalizado)
            {
                MessageBox.Show("Pedido finalizado não pode ser editado.");
                return;
            }

            EditBuffer = new Pedido
            {
                Id = PedidoSelecionado.Id,
                PessoaId = PedidoSelecionado.PessoaId,
                PessoaNome = PedidoSelecionado.PessoaNome,
                DataVenda = PedidoSelecionado.DataVenda,
                Status = PedidoSelecionado.Status,
                ValorTotal = PedidoSelecionado.ValorTotal,
                FormaPagamento = PedidoSelecionado.FormaPagamento,
                IsFinalizado = PedidoSelecionado.IsFinalizado,
                Itens = PedidoSelecionado.Itens?.Select(i => new PedidoItem
                {
                    ProdutoId = i.ProdutoId,
                    ProdutoNome = i.ProdutoNome,
                    Quantidade = i.Quantidade,
                    ValorUnitario = i.ValorUnitario
                }).ToList() ?? new()
            };

            ItensEdit.Clear();
            foreach (var it in EditBuffer.Itens) ItensEdit.Add(it);

            PessoaSelecionadaEdicao = Pessoas.FirstOrDefault(p => p.Id == EditBuffer.PessoaId);
            QtdeParaAdicionar = 1;
            ProdutoSelecionadoParaAdicionar = null;
            RecalcularTotal();
            IsEditando = true;
        }

        private void Salvar()
        {
            try
            {
                // Bloquear salvar alterações se o pedido já foi finalizado
                if (EditBuffer.IsFinalizado)
                {
                    MessageBox.Show("Pedido finalizado não pode ser alterado.");
                    return;
                }

                EditBuffer.Itens = ItensEdit.ToList();

                if (EditBuffer.Id == 0 && !EditBuffer.DataVenda.HasValue)
                    EditBuffer.DataVenda = DateTime.Now;

                RecalcularTotal();

                if (EditBuffer.PessoaId <= 0)
                {
                    MessageBox.Show("Selecione uma pessoa.");
                    return;
                }
                if (EditBuffer.Itens.Count == 0)
                {
                    MessageBox.Show("Adicione ao menos um item.");
                    return;
                }

                if (EditBuffer.Id == 0)
                {
                    var adicionado = _pedidosRepo.Add(EditBuffer);
                    Pedidos.Insert(0, adicionado);
                    PedidoSelecionado = adicionado;
                }
                else
                {
                    _pedidosRepo.Update(EditBuffer);
                    var idx = Pedidos.ToList().FindIndex(p => p.Id == EditBuffer.Id);
                    if (idx >= 0)
                    {
                        Pedidos[idx] = EditBuffer;
                        PedidoSelecionado = EditBuffer;
                    }
                }

                IsEditando = false;
                IsModalPedidoVisible = false;
                ItensEdit.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar pedido: {ex.Message}");
            }
        }

        private void Excluir()
        {
            if (PedidoSelecionado == null) return;
            if (MessageBox.Show($"Excluir pedido #{PedidoSelecionado.Id} de {PedidoSelecionado.PessoaNome}?",
                                "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            if (_pedidosRepo.Delete(PedidoSelecionado.Id))
            {
                Pedidos.Remove(PedidoSelecionado);
                PedidoSelecionado = null;
            }
        }

        // Itens
        private PedidoItem? _itemSelecionado;
        public PedidoItem? ItemSelecionado
        {
            get => _itemSelecionado;
            set
            {
                if (SetProperty(ref _itemSelecionado, value))
                {
                    RemoverItemCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private void AdicionarItem()
        {
            if (ProdutoSelecionadoParaAdicionar == null || QtdeParaAdicionar <= 0) return;

            var existente = ItensEdit.FirstOrDefault(i => i.ProdutoId == ProdutoSelecionadoParaAdicionar.Id);
            if (existente != null)
            {
                existente.Quantidade += QtdeParaAdicionar;
            }
            else
            {
                ItensEdit.Add(new PedidoItem
                {
                    ProdutoId = ProdutoSelecionadoParaAdicionar.Id,
                    ProdutoNome = ProdutoSelecionadoParaAdicionar.Nome,
                    Quantidade = QtdeParaAdicionar,
                    ValorUnitario = ProdutoSelecionadoParaAdicionar.Valor
                });
            }

            RecalcularTotal();
            QtdeParaAdicionar = 1;
            SalvarCommand.RaiseCanExecuteChanged();
            FinalizarCommand.RaiseCanExecuteChanged();
        }

        private void RemoverItem()
        {
            if (ItemSelecionado == null) return;
            ItensEdit.Remove(ItemSelecionado);
            RecalcularTotal();
            SalvarCommand.RaiseCanExecuteChanged();
            FinalizarCommand.RaiseCanExecuteChanged();
        }

        private void Finalizar()
        {
            try
            {
                EditBuffer.Itens = ItensEdit.ToList();

                if (!EditBuffer.DataVenda.HasValue)
                    EditBuffer.DataVenda = DateTime.Now;

                RecalcularTotal();

                // Manter status Pendente por padrão ao finalizar; sem mudança automática para Pago

                EditBuffer.IsFinalizado = true;

                if (EditBuffer.Id == 0)
                {
                    var adicionado = _pedidosRepo.Add(EditBuffer);
                    Pedidos.Insert(0, adicionado);
                    PedidoSelecionado = adicionado;
                }
                else
                {
                    _pedidosRepo.Update(EditBuffer);
                    var idx = Pedidos.ToList().FindIndex(p => p.Id == EditBuffer.Id);
                    if (idx >= 0) Pedidos[idx] = EditBuffer;
                }

                MessageBox.Show("Pedido finalizado.");
                IsEditando = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao finalizar: {ex.Message}");
            }
        }

        private void FinalizarSelecionado()
        {
            try
            {
                if (PedidoSelecionado == null) return;
                if (PedidoSelecionado.IsFinalizado)
                {
                    MessageBox.Show("Pedido já finalizado.");
                    return;
                }

                if (!PedidoSelecionado.DataVenda.HasValue)
                    PedidoSelecionado.DataVenda = DateTime.Now;

                // Recalcular total baseado nos itens atuais
                var itens = PedidoSelecionado.Itens ?? new System.Collections.Generic.List<PedidoItem>();
                PedidoSelecionado.ValorTotal = itens.Sum(i => i.TotalItem);

                // Manter status atual (padrão: Pendente) e apenas marcar como finalizado
                PedidoSelecionado.IsFinalizado = true;

                _pedidosRepo.Update(PedidoSelecionado);

                var idx = Pedidos.ToList().FindIndex(p => p.Id == PedidoSelecionado.Id);
                if (idx >= 0)
                    Pedidos[idx] = PedidoSelecionado;

                MessageBox.Show("Pedido finalizado.");
                FinalizarSelecionadoCommand.RaiseCanExecuteChanged();
                EditarCommand.RaiseCanExecuteChanged();
                MarcarPagoCommand.RaiseCanExecuteChanged();
                MarcarEnviadoCommand.RaiseCanExecuteChanged();
                MarcarRecebidoCommand.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao finalizar: {ex.Message}");
            }
        }

        private void MarcarStatus(StatusPedido novoStatus)
        {
            try
            {
                if (PedidoSelecionado == null) return;

                if (!PedidoSelecionado.IsFinalizado)
                {
                    MessageBox.Show("Finalize o pedido antes de alterar o status.");
                    return;
                }

                if (PedidoSelecionado.Status == novoStatus) return;

                PedidoSelecionado.Status = novoStatus;
                _pedidosRepo.Update(PedidoSelecionado);

                var idx = Pedidos.ToList().FindIndex(p => p.Id == PedidoSelecionado.Id);
                if (idx >= 0)
                    Pedidos[idx] = PedidoSelecionado;

                MarcarPagoCommand.RaiseCanExecuteChanged();
                MarcarEnviadoCommand.RaiseCanExecuteChanged();
                MarcarRecebidoCommand.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao atualizar status: {ex.Message}");
            }
        }
    }
}