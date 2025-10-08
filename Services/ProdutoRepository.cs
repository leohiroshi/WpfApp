using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp.Models;

namespace WpfApp.Services
{
    public class ProdutoRepository
    {
        private readonly JsonRepository<Produto> _repo;

        public ProdutoRepository(string filePath)
        {
            _repo = new JsonRepository<Produto>(filePath);
        }

        public IEnumerable<Produto> GetAll() => _repo.GetAll();
        public Produto Add(Produto p) => _repo.Add(p);
        public void Update(Produto p) => _repo.Update(p);
        public bool Delete(int id) => _repo.Delete(id);
        public Produto GetById(int id) => _repo.GetById(id);

        // Filtros: Nome, Código, Faixa de Valor
        public IEnumerable<Produto> Buscar(string nome, string codigo, decimal? valorMin, decimal? valorMax)
        {
            var q = _repo.GetAll().AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
                q = q.Where(p => p.Nome != null &&
                                 p.Nome.IndexOf(nome, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrWhiteSpace(codigo))
                q = q.Where(p => p.Codigo != null &&
                                 p.Codigo.IndexOf(codigo, StringComparison.OrdinalIgnoreCase) >= 0);

            if (valorMin.HasValue)
                q = q.Where(p => p.Valor >= valorMin.Value);

            if (valorMax.HasValue)
                q = q.Where(p => p.Valor <= valorMax.Value);

            return q.OrderBy(p => p.Nome ?? string.Empty).ToList();
        }
    }
}