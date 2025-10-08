using WpfApp.Services;

namespace WpfApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public PessoasViewModel PessoasVM { get; }
        public ProdutosViewModel ProdutosVM { get; }
        // Se tiver uma aba “Pedidos”, crie também um PedidosViewModel

        public MainViewModel(PessoaRepository pessoaRepo,
                             ProdutoRepository produtoRepo,
                             PedidoRepository pedidoRepo)
        {
            PessoasVM = new PessoasViewModel(pessoaRepo, produtoRepo, pedidoRepo);
            ProdutosVM = new ProdutosViewModel(produtoRepo);
        }
    }
}