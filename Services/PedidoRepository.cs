using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp.Models;

namespace WpfApp.Services
{
    public class PedidoRepository
    {
        private readonly JsonRepository<Pedido> _repo;

        public PedidoRepository(string filePath)
        {
            _repo = new JsonRepository<Pedido>(filePath);
        }

        public IEnumerable<Pedido> GetAll() => _repo.GetAll();
        public Pedido Add(Pedido p) => _repo.Add(p);
        public void Update(Pedido p) => _repo.Update(p);
        public bool Delete(int id) => _repo.Delete(id);
        public Pedido GetById(int id) => _repo.GetById(id);

        // Consultas por pessoa e status com LINQ
        public IEnumerable<Pedido> BuscarPorPessoa(int pessoaId) =>
            _repo.GetAll().Where(p => p.PessoaId == pessoaId)
                          .OrderByDescending(p => p.DataVenda);

        public IEnumerable<Pedido> FiltrarPorStatus(int pessoaId, StatusPedido? status)
        {
            var q = _repo.GetAll().Where(p => p.PessoaId == pessoaId);
            if (status.HasValue)
                q = q.Where(p => p.Status == status.Value);
            return q.OrderByDescending(p => p.DataVenda);
        }

        public IEnumerable<Pedido> SomenteEntregues(int pessoaId) =>
            FiltrarPorStatus(pessoaId, StatusPedido.Recebido);

        public IEnumerable<Pedido> SomentePagos(int pessoaId) =>
            FiltrarPorStatus(pessoaId, StatusPedido.Pago);

        public IEnumerable<Pedido> SomentePendentesPagamento(int pessoaId) =>
            FiltrarPorStatus(pessoaId, StatusPedido.Pendente);
    }
}