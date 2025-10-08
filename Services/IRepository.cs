using System;
using System.Collections.Generic;

namespace WpfApp.Services
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        T Add(T entity);
        void Update(T entity);
        bool Delete(int id);

        int NextId(Func<T, int> idSelector);
    }
}