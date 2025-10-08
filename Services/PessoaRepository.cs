using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WpfApp.Models;

namespace WpfApp.Services
{
    public class PessoaRepository
    {
        private readonly JsonRepository<Pessoa> _repo;

        public PessoaRepository(string filePath)
        {
            _repo = new JsonRepository<Pessoa>(filePath);
        }

        public IEnumerable<Pessoa> GetAll() => _repo.GetAll();

        public Pessoa Add(Pessoa p) => _repo.Add(p);

        public void Update(Pessoa p) => _repo.Update(p);

        public bool Delete(int id) => _repo.Delete(id);

        public Pessoa GetById(int id) => _repo.GetById(id);

        // Exemplos de filtros com LINQ
        public IEnumerable<Pessoa> Buscar(string nome, string cpf)
        {
            var q = _repo.GetAll().AsQueryable();
            if (!string.IsNullOrWhiteSpace(nome))
                q = q.Where(p => p.Nome != null &&
                                 p.Nome.IndexOf(nome, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrWhiteSpace(cpf))
                q = q.Where(p => p.Cpf != null &&
                                 p.Cpf.IndexOf(cpf, StringComparison.OrdinalIgnoreCase) >= 0);

            return q.OrderBy(p => p.Nome ?? string.Empty).ToList();
        }
    }
}