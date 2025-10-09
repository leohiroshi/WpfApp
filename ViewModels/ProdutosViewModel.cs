using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WpfApp.Infra;
using WpfApp.Models;
using WpfApp.Services;

namespace WpfApp.ViewModels
{
    public class ProdutosViewModel : BaseViewModel
    {
        private readonly ProdutoRepository _produtosRepo;

        public ObservableCollection<Produto> Produtos { get; } = new();

        private Produto _produtoSelecionado;
        public Produto? ProdutoSelecionado
        {
            get => _produtoSelecionado;
            set => SetProperty(ref _produtoSelecionado, value);
        }

        // Filtros
        private string _filtroNome = string.Empty;
        public string FiltroNome
        {
            get => _filtroNome;
            set => SetProperty(ref _filtroNome, value);
        }

        private string _filtroCodigo = string.Empty;
        public string FiltroCodigo
        {
            get => _filtroCodigo;
            set => SetProperty(ref _filtroCodigo, value);
        }

        private string _filtroValorMin = string.Empty;
        public string FiltroValorMin
        {
            get => _filtroValorMin;
            set => SetProperty(ref _filtroValorMin, value);
        }

        private string _filtroValorMax = string.Empty;
        public string FiltroValorMax
        {
            get => _filtroValorMax;
            set => SetProperty(ref _filtroValorMax, value);
        }

        // Buffer de edição
        private Produto _editBuffer = new();
        public Produto EditBuffer
        {
            get => _editBuffer;
            set => SetProperty(ref _editBuffer, value);
        }

        // Estado
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

        // Comandos
        public RelayCommand CarregarCommand { get; }
        public RelayCommand BuscarCommand { get; }
        public RelayCommand IncluirCommand { get; }
        public RelayCommand EditarCommand { get; }
        public RelayCommand SalvarCommand { get; }
        public RelayCommand ExcluirCommand { get; }

        public ProdutosViewModel(ProdutoRepository produtosRepo)
        {
            _produtosRepo = produtosRepo;

            CarregarCommand = new RelayCommand(CarregarProdutos);
            BuscarCommand = new RelayCommand(Buscar);
            IncluirCommand = new RelayCommand(Incluir, () => !IsEditando);
            EditarCommand = new RelayCommand(Editar, () => ProdutoSelecionado != null && !IsEditando);
            SalvarCommand = new RelayCommand(Salvar, () => IsEditando);
            ExcluirCommand = new RelayCommand(Excluir, () => ProdutoSelecionado != null && !IsEditando);

            CarregarProdutos();
        }

        private void CarregarProdutos()
        {
            Produtos.Clear();
            foreach (var p in _produtosRepo.GetAll())
                Produtos.Add(p);
        }

        private void Buscar()
        {
            decimal? valorMin = null;
            decimal? valorMax = null;

            if (!string.IsNullOrWhiteSpace(FiltroValorMin) && decimal.TryParse(FiltroValorMin, out var min))
                valorMin = min;

            if (!string.IsNullOrWhiteSpace(FiltroValorMax) && decimal.TryParse(FiltroValorMax, out var max))
                valorMax = max;

            Produtos.Clear();
            foreach (var p in _produtosRepo.Buscar(FiltroNome, FiltroCodigo, valorMin, valorMax))
                Produtos.Add(p);
        }

        private void Incluir()
        {
            EditBuffer = new Produto();
            IsEditando = true;
        }

        private void Editar()
        {
            if (ProdutoSelecionado == null) return;

            EditBuffer = new Produto
            {
                Id = ProdutoSelecionado.Id,
                Nome = ProdutoSelecionado.Nome,
                Codigo = ProdutoSelecionado.Codigo,
                Valor = ProdutoSelecionado.Valor
            };
            IsEditando = true;
        }

        private void Salvar()
        {
            try
            {
                // Validações
                if (string.IsNullOrWhiteSpace(EditBuffer.Nome))
                {
                    MessageBox.Show("Nome é obrigatório.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(EditBuffer.Codigo))
                {
                    MessageBox.Show("Código é obrigatório.");
                    return;
                }

                if (EditBuffer.Valor <= 0)
                {
                    MessageBox.Show("Valor deve ser maior que zero.");
                    return;
                }

                if (EditBuffer.Id == 0)
                {
                    // Inclusão
                    var adicionado = _produtosRepo.Add(EditBuffer);
                    Produtos.Add(adicionado);
                    ProdutoSelecionado = adicionado;
                }
                else
                {
                    // Edição
                    _produtosRepo.Update(EditBuffer);
                    var idx = Produtos.ToList().FindIndex(p => p.Id == EditBuffer.Id);
                    if (idx >= 0)
                    {
                        Produtos[idx] = EditBuffer;
                        ProdutoSelecionado = EditBuffer;
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
            if (ProdutoSelecionado == null) return;

            if (MessageBox.Show($"Excluir produto '{ProdutoSelecionado.Nome}'?", "Confirmação",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            if (_produtosRepo.Delete(ProdutoSelecionado.Id))
            {
                Produtos.Remove(ProdutoSelecionado);
                ProdutoSelecionado = null;
            }
        }
    }
}