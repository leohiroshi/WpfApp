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
            set => SetProperty(ref _pessoaSelecionada, value);
        }

        // Filtros
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

        // Campos de edição (binding com TextBoxes)
        private Pessoa _editBuffer = new();
        public Pessoa EditBuffer
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
        public RelayCommand IncluirPessoaTesteCommand { get; }

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

            IncluirPessoaTesteCommand = new RelayCommand(AdicionarPessoaTeste);

            CarregarPessoas();
        }

        private void CarregarPessoas()
        {
            Pessoas.Clear();
            foreach (var p in _pessoasRepo.GetAll())
                Pessoas.Add(p);
        }

        private void Buscar()
        {
            Pessoas.Clear();
            foreach (var p in _pessoasRepo.Buscar(FiltroNome, FiltroCpf))
                Pessoas.Add(p);
        }

        private void Incluir()
        {
            EditBuffer = new Pessoa(); // limpa buffer
            IsEditando = true;
        }

        private void Editar()
        {
            if (PessoaSelecionada == null) return;
            // cria uma cópia para editar sem sujar a seleção
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
                // validações básicas
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
                    // inclusão
                    var adicionada = _pessoasRepo.Add(EditBuffer);
                    Pessoas.Add(adicionada);
                    PessoaSelecionada = adicionada;
                }
                else
                {
                    // edição
                    _pessoasRepo.Update(EditBuffer);
                    // reflete na lista
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
            }
        }

        // método de teste que você já tinha
        public void AdicionarPessoaTeste()
        {
            try
            {
                var novaPessoa = new Pessoa
                {
                    Nome = "Leonardo Hiroshi Silva",
                    Cpf = "12924013976",
                    Endereco = "Rua José Elezeu Ribeiro, 48 - Abranches"
                };
                _pessoasRepo.Add(novaPessoa);
                MessageBox.Show($"Pessoa '{novaPessoa.Nome}' adicionada (Id: {novaPessoa.Id})");
                Pessoas.Add(novaPessoa);
                PessoaSelecionada = novaPessoa;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao adicionar pessoa: {ex.Message}");
            }
        }
    }
}