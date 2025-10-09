using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using WpfApp.ViewModels;
using WpfApp.Views;
using WpfApp.Services;
using WpfApp.Models;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
    public MainViewModel? MainViewModel { get; private set; }

        // Controle de seção ativa
        private string _secaoAtiva = "Pessoas";
        public string SecaoAtiva
        {
            get => _secaoAtiva;
            set
            {
                _secaoAtiva = value;
                AtualizarVisibilidadeSecoes();
            }
        }

        public MainWindow()
        {
            // Carregar XAML manualmente (equivalente a InitializeComponent)
            var resourceLocater = new Uri("/WpfApp;component/Views/MainWindow.xaml", UriKind.Relative);
            Application.LoadComponent(this, resourceLocater);

            // Garantir que o DataContext vindo do App.xaml.cs seja associado ao MainViewModel
            this.Loaded += (s, e) =>
            {
                MainViewModel ??= this.DataContext as MainViewModel;
                // Carregar dados iniciais quando a janela estiver pronta
                CarregarDadosIniciais();
            };
        }

        // Método InitializeComponent é gerado automaticamente pelo WPF em MainWindow.g.cs

        private void CarregarDadosIniciais()
        {
            // Carregar pessoas por padrão
            MainViewModel?.PessoasVM.CarregarCommand.Execute(null);

            // Definir visibilidade inicial
            AtualizarVisibilidadeSecoes();
        }

        private void AtualizarVisibilidadeSecoes()
        {
            // Atualizar visibilidade das seções baseado na seção ativa usando FindName para evitar dependência em campos gerados
            if (FindName("pessoasSection") is FrameworkElement pessoas)
                pessoas.Visibility = SecaoAtiva == "Pessoas" ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("produtosSection") is FrameworkElement produtos)
                produtos.Visibility = SecaoAtiva == "Produtos" ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("pedidosSection") is FrameworkElement pedidos)
                pedidos.Visibility = SecaoAtiva == "Pedidos" ? Visibility.Visible : Visibility.Collapsed;

            // Atualizar título da página
            if (FindName("pageTitle") is TextBlock title)
            {
                title.Text = SecaoAtiva switch
                {
                    "Pessoas" => "Pessoas",
                    "Produtos" => "Produtos",
                    "Pedidos" => "Pedidos",
                    _ => "Members"
                };
            }
        }

        private void BtnFecharApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Event handlers para navegação
        private void MenuPessoas_Click(object sender, RoutedEventArgs e)
        {
            SecaoAtiva = "Pessoas";
            MainViewModel?.PessoasVM.CarregarCommand.Execute(null);
        }

        private void MenuProdutos_Click(object sender, RoutedEventArgs e)
        {
            SecaoAtiva = "Produtos";
            MainViewModel?.ProdutosVM.CarregarCommand.Execute(null);
        }

        private void MenuPedidos_Click(object sender, RoutedEventArgs e)
        {
            SecaoAtiva = "Pedidos";
            MainViewModel?.PedidosVM.CarregarCommand.Execute(null);
        }

        // Ao clicar em "Incluir Pedido" na aba Pessoas, navegar para Pedidos e iniciar um novo pedido para a pessoa selecionada
        private void BtnIncluirPedidoPessoas_Click(object sender, RoutedEventArgs e)
        {
            var pessoa = MainViewModel?.PessoasVM.PessoaSelecionada;
            if (pessoa == null)
            {
                MessageBox.Show("Selecione uma pessoa antes de incluir um pedido.");
                return;
            }

            // Navegar para a seção de Pedidos
            SecaoAtiva = "Pedidos";

            // Preparar um novo pedido no ViewModel de Pedidos
            var pedidosVM = MainViewModel?.PedidosVM;
            if (pedidosVM == null) return;

            // Abrir modal de pedido (chama Incluir() internamente)
            pedidosVM.AbrirModalPedidoCommand.Execute(null);

            // Selecionar a pessoa no pedido em edição (após abrir o modal para não ser sobrescrito pelo Incluir())
            var pessoaVMMatch = pedidosVM.Pessoas.FirstOrDefault(p => p.Id == pessoa.Id);
            if (pessoaVMMatch != null)
            {
                pedidosVM.PessoaSelecionadaEdicao = pessoaVMMatch;
            }
        }

        // Métodos existentes para window manipulation
        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private bool IsMaximized = false;
        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (IsMaximized)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1080;
                    this.Height = 720;
                    IsMaximized = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    IsMaximized = true;
                }
            }
        }
    }
}
