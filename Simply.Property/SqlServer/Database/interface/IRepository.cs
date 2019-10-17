using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simply.Property.SqlServer
{
    /// <summary>
    /// Фасад доступа к базе данных
    /// </summary>
    public interface IRepository : IDisposable
    {
        /// <summary>
        /// Выполнить sql запрос
        /// </summary>
        /// <param name="sql">sql запрос</param>
        /// <param name="json">параметр запроса json (может быть null)</param>
        /// <returns></returns>
        Task<int> ExecuteSqlAsync(string sql, string json = null);
        /// <summary>
        /// Выполнить несколько запросов в одной транзакции
        /// </summary>
        /// <param name="queries">sql запросы</param>
        /// <returns></returns>
        Task ExecuteSqlAsync(IEnumerable<SqlQuery> queries);
        /// <summary>
        /// Добавить объект в контекст
        /// </summary>
        /// <typeparam name="T">тип объекта</typeparam>
        /// <param name="entity">объект, который необходимо довабить</param>
        /// <returns></returns>
        Task<int> Add<T>(T entity) where T : class;
        /// <summary>
        /// Загрузить данные
        /// </summary>
        /// <typeparam name="T">тип загружаемых данных (ассоциированный с таблицей базы данных)</typeparam>
        /// <returns></returns>
        Task<List<T>> GetAsync<T>() where T : class;
        /// <summary>
        /// Сохранить изменения контекста в базе данных
        /// </summary>
        /// <returns></returns>
        Task<int> SaveAsync();
    }
}
