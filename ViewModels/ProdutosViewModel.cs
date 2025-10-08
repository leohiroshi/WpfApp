using WpfApp.Services;

namespace WpfApp.ViewModels
{
    public class ProdutosViewModel : BaseViewModel
    {
        private readonly ProdutoRepository _produtos;

        public ProdutosViewModel(ProdutoRepository produtos)
        {
            _produtos = produtos;
        }
    }
}