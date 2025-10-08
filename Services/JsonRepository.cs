using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WpfApp.Services
{
    public class JsonRepository<T> : IRepository<T> where T : class
    {
        private readonly string _filePath;
        private readonly object _lock = new object();

        public JsonRepository(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            EnsureFile();
        }

        private void EnsureFile()
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        private List<T> LoadAllInternal()
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_filePath);
                var list = JsonConvert.DeserializeObject<List<T>>(json);
                return list ?? new List<T>();
            }
        }

        private void SaveAllInternal(List<T> list)
        {
            lock (_lock)
            {
                var json = JsonConvert.SerializeObject(list, Formatting.Indented);
                File.WriteAllText(_filePath, json);
            }
        }

        public IEnumerable<T> GetAll() => LoadAllInternal();

        public T GetById(int id)
        {
            var list = LoadAllInternal();
            var prop = typeof(T).GetProperty("Id");
            if (prop == null) throw new InvalidOperationException("Tipo não possui propriedade Id.");
            return list.FirstOrDefault(e => (int)(prop.GetValue(e) ?? 0) == id)
                   ?? throw new InvalidOperationException($"Entidade com Id {id} não encontrada.");
        }

        public T Add(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var prop = typeof(T).GetProperty("Id");
            if (prop == null) throw new InvalidOperationException("Tipo não possui propriedade Id.");

            var list = LoadAllInternal();
            // Se Id == 0, gera próximo Id (Max + 1)
            var currentId = (int)(prop.GetValue(entity) ?? 0);
            if (currentId == 0)
            {
                var next = list.Any()
                    ? list.Max(x => (int)(prop.GetValue(x) ?? 0)) + 1
                    : 1;
                prop.SetValue(entity, next);
            }
            else
            {
                // Se já veio com Id, garanta que não colida
                if (list.Any(x => (int)(prop.GetValue(x) ?? 0) == currentId))
                    throw new InvalidOperationException($"Já existe entidade com Id {currentId}.");
            }

            list.Add(entity);
            SaveAllInternal(list);
            return entity;
        }

        public void Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var prop = typeof(T).GetProperty("Id");
            if (prop == null) throw new InvalidOperationException("Tipo não possui propriedade Id.");

            var id = (int)(prop.GetValue(entity) ?? 0);
            if (id == 0) throw new InvalidOperationException("Id inválido para Update.");

            var list = LoadAllInternal();
            var idx = list.FindIndex(e => (int)(prop.GetValue(e) ?? 0) == id);
            if (idx < 0) throw new KeyNotFoundException($"Entidade Id {id} não encontrada.");

            list[idx] = entity;
            SaveAllInternal(list);
        }

        public bool Delete(int id)
        {
            var list = LoadAllInternal();
            var prop = typeof(T).GetProperty("Id");
            if (prop == null) throw new InvalidOperationException("Tipo não possui propriedade Id.");

            var removed = list.RemoveAll(e => (int)(prop.GetValue(e) ?? 0) == id);
            if (removed > 0)
            {
                SaveAllInternal(list);
                return true;
            }
            return false;
        }

        public int NextId(Func<T, int> idSelector)
        {
            var list = LoadAllInternal();
            return list.Any() ? list.Max(idSelector) + 1 : 1;
        }
    }
}