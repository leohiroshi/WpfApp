using WpfApp.Services;
using WpfApp.ViewModels;

public class MainViewModel : BaseViewModel
{
    public PessoasViewModel PessoasVM { get; }
    public ProdutosViewModel ProdutosVM { get; }
    public PedidosViewModel PedidosVM { get; }

    public MainViewModel(PessoaRepository pessoaRepo,
                         ProdutoRepository produtoRepo,
                         PedidoRepository pedidoRepo)
    {
        var pedidoService = new PedidoService();

        PessoasVM = new PessoasViewModel(pessoaRepo, produtoRepo, pedidoRepo);
        ProdutosVM = new ProdutosViewModel(produtoRepo);
        PedidosVM = new PedidosViewModel(pedidoRepo, pessoaRepo, produtoRepo, pedidoService);
    }
}