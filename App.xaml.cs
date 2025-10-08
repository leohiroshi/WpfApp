using System;
using System.IO;
using System.Windows;
using WpfApp.Services;
using WpfApp.ViewModels;
using WpfApp.Views;

namespace WpfApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(baseDir, "Data");
            Directory.CreateDirectory(dataDir);

            var pessoaRepo = new PessoaRepository(Path.Combine(dataDir, "pessoas.json"));
            var produtoRepo = new ProdutoRepository(Path.Combine(dataDir, "produtos.json"));
            var pedidoRepo = new PedidoRepository(Path.Combine(dataDir, "pedidos.json"));

            var mainVM = new MainViewModel(pessoaRepo, produtoRepo, pedidoRepo);

            var mainWindow = new MainWindow
            {
                DataContext = mainVM
            };
            mainWindow.Show();
        }
    }
}