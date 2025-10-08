using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class PedidoItem
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public decimal ValorUnitario { get; set; }
        public int Quantidade { get; set; }
        public decimal TotalItem => ValorUnitario * Quantidade;
    }

    public class Pedido
    {
        public int Id { get; set; }
        public int PessoaId { get; set; }
        public List<PedidoItem> Itens { get; set; } = new();
        public decimal ValorTotal { get; set; }
        public DateTime DataVenda { get; set; }
        public FormaPagamento FormaPagamento { get; set; }
        public StatusPedido Status { get; set; } = StatusPedido.Pendente;
        public bool IsFinalizado { get; set; }
    }
}
