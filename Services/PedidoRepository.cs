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

        // CRUD básico
        public IEnumerable<Pedido> GetAll() => _repo.GetAll();
        public Pedido GetById(int id) => _repo.GetById(id);

        public Pedido Add(Pedido p)
        {
            return _repo.Add(p);
        }

        public void Update(Pedido p)
        {
            _repo.Update(p);
        }

        public bool Delete(int id) => _repo.Delete(id);

        // Consultas específicas
        public IEnumerable<Pedido> BuscarPorPessoa(int pessoaId) =>
            _repo.GetAll()
                 .Where(p => p.PessoaId == pessoaId)
                 .OrderByDescending(p => p.DataVenda ?? DateTime.MinValue);

        public IEnumerable<Pedido> FiltrarPorStatus(int pessoaId, StatusPedido? status)
        {
            var q = _repo.GetAll().Where(p => p.PessoaId == pessoaId);
            if (status.HasValue)
                q = q.Where(p => p.Status == status.Value);

            return q.OrderByDescending(p => p.DataVenda ?? DateTime.MinValue);
        }

        public IEnumerable<Pedido> SomenteEntregues(int pessoaId) =>
            FiltrarPorStatus(pessoaId, StatusPedido.Recebido);

        public IEnumerable<Pedido> SomentePagos(int pessoaId) =>
            FiltrarPorStatus(pessoaId, StatusPedido.Pago);

        public IEnumerable<Pedido> SomentePendentesPagamento(int pessoaId) =>
            FiltrarPorStatus(pessoaId, StatusPedido.Pendente);

        // Filtros gerais usados na tela (pessoa por nome, status, intervalo de data)
        public IEnumerable<Pedido> Buscar(string? pessoaNome, StatusPedido? status, DateTime? dataIni, DateTime? dataFim)
        {
            var q = _repo.GetAll().AsQueryable();

            if (!string.IsNullOrWhiteSpace(pessoaNome))
                q = q.Where(p => (p.PessoaNome ?? string.Empty)
                        .Contains(pessoaNome, StringComparison.OrdinalIgnoreCase));

            if (status.HasValue)
                q = q.Where(p => p.Status == status.Value);

            if (dataIni.HasValue)
            {
                var di = dataIni.Value.Date;
                q = q.Where(p => (p.DataVenda ?? DateTime.MinValue).Date >= di);
            }

            if (dataFim.HasValue)
            {
                var df = dataFim.Value.Date;
                q = q.Where(p => (p.DataVenda ?? DateTime.MinValue).Date <= df);
            }

            return q.OrderByDescending(p => p.DataVenda ?? DateTime.MinValue);
        }

        // Versão por pessoaId + filtros (útil quando navega pela pessoa)
        public IEnumerable<Pedido> BuscarPorPessoaComFiltros(int pessoaId, StatusPedido? status, DateTime? dataIni, DateTime? dataFim)
        {
            var q = _repo.GetAll().Where(p => p.PessoaId == pessoaId);

            if (status.HasValue)
                q = q.Where(p => p.Status == status.Value);

            if (dataIni.HasValue)
            {
                var di = dataIni.Value.Date;
                q = q.Where(p => (p.DataVenda ?? DateTime.MinValue).Date >= di);
            }

            if (dataFim.HasValue)
            {
                var df = dataFim.Value.Date;
                q = q.Where(p => (p.DataVenda ?? DateTime.MinValue).Date <= df);
            }

            return q.OrderByDescending(p => p.DataVenda ?? DateTime.MinValue);
        }
    }
}