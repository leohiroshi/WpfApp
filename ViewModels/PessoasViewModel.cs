using System.Windows;
using WpfApp.Services;
using WpfApp.Infra;

namespace WpfApp.ViewModels
{
    public class PessoasViewModel : BaseViewModel
    {
        private readonly PessoaRepository _pessoasRepo;

        public RelayCommand AdicionarPessoaTesteCommand { get; }

        public PessoasViewModel(PessoaRepository pessoasRepo, ProdutoRepository produtosRepo, PedidoRepository pedidosRepo)
        {
            _pessoasRepo = pessoasRepo;

            AdicionarPessoaTesteCommand = new RelayCommand(AdicionarPessoaTeste);
        }

        public void AdicionarPessoaTeste()
        {
            try
            {
                var novaPessoa = new Models.Pessoa
                {
                    Nome = "Leonardo Hiroshi Silva",
                    Cpf = "12924013976", // Use um CPF válido
                    Endereco = "Rua José Elezeu Ribeiro, 48 - Abranches"
                };
                _pessoasRepo.Add(novaPessoa);
                MessageBox.Show($"Pessoa '{novaPessoa.Nome}' adicionada com Id: {novaPessoa.Id}");
                // Recarregar a lista de pessoas no VM se você já tiver uma ObservableCollection
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao adicionar pessoa: {ex.Message}");
            }
        }
    }
}