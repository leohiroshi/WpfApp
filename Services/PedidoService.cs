using System;
using System.Linq;
using WpfApp.Models;

namespace WpfApp.Services
{
    public class PedidoService
    {
        public void RecalcularTotal(Pedido pedido)
        {
            if (pedido.Itens == null) { pedido.ValorTotal = 0m; return; }
            pedido.ValorTotal = pedido.Itens.Sum(i => i.Quantidade * i.ValorUnitario);
        }

        public void SincronizarValoresItem(PedidoItem item, Produto produto)
        {
            if (item == null || produto == null) return;
            item.ProdutoId = produto.Id;
            item.ProdutoNome = produto.Nome;
            item.ValorUnitario = produto.Valor;
        }

        public void ValidarPedidoParaSalvar(Pedido pedido)
        {
            if (pedido == null) throw new ArgumentNullException(nameof(pedido));
            if (pedido.PessoaId <= 0) throw new InvalidOperationException("Selecione uma pessoa.");
            if (pedido.Itens == null || pedido.Itens.Count == 0) throw new InvalidOperationException("Adicione ao menos um item.");
            if (pedido.Itens.Any(i => i.Quantidade <= 0)) throw new InvalidOperationException("Quantidade deve ser maior que zero.");
            if (pedido.Itens.Any(i => i.ValorUnitario  <= 0m)) throw new InvalidOperationException("Valor unitário inválido.");
        }

        public void PrepararFinalizacao(Pedido pedido)
        {
            pedido.DataVenda = DateTime.Now;
        }
    }
}