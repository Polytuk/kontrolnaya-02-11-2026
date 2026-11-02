using System.Collections.Generic;

namespace BodagovKR1_.Interfaces
{
    /// <summary>
    /// Обобщенный интерфейс репозитория с ограничением where T : class.
    /// Предоставляет стандартные операции CRUD.
    /// </summary>
    /// <typeparam name="T">Ссылочный тип сущности предметной области.</typeparam>
    public interface IRepository<T> where T : class
    {
        List<T> GetAll();
        T? GetById(int id);
        void Add(T item);
        bool Delete(int id);
    }
}

