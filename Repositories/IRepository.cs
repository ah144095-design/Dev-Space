using System.Collections.Generic;

namespace DevSpace.Repositories
{
    public interface IRepository<T>
    {
        void Add(T item);
        void Update(T item);
        void Delete(int id);
        T GetById(int id);
        List<T> GetAll();
    }
}
