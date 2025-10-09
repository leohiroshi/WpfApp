using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WpfApp.Models
{
    public enum FormaPagamento
    {
        Dinheiro = 0,
        Cartao = 1,
        Boleto = 2
    }

    public enum StatusPedido
    {
        Pendente = 0,
        Pago = 1,
        Enviado = 2,
        Recebido = 3
    }

    public class PedidoItem : INotifyPropertyChanged
    {
        private int _quantidade;
        private decimal _valorUnitario;

        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;

        public decimal ValorUnitario
        {
            get => _valorUnitario;
            set
            {
                if (_valorUnitario != value)
                {
                    _valorUnitario = value;
                    OnPropertyChanged(nameof(ValorUnitario));
                    OnPropertyChanged(nameof(TotalItem));
                }
            }
        }

        public int Quantidade
        {
            get => _quantidade;
            set
            {
                if (_quantidade != value)
                {
                    _quantidade = value;
                    OnPropertyChanged(nameof(Quantidade));
                    OnPropertyChanged(nameof(TotalItem));
                }
            }
        }

        public decimal TotalItem => ValorUnitario * Quantidade;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class Pedido
    {
        public int Id { get; set; }
        public int PessoaId { get; set; }
        public string? PessoaNome { get; set; }
        public List<PedidoItem> Itens { get; set; } = new();
        public decimal ValorTotal { get; set; }
        public DateTime? DataVenda { get; set; }
        public FormaPagamento FormaPagamento { get; set; }
        public StatusPedido Status { get; set; } = StatusPedido.Pendente;
        public bool IsFinalizado { get; set; }
    }
}