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
        /// <param name="query">sql запрос</param>
        /// <returns>Возвращает количество строк, измененных в дазе данных</returns>
        Task<int> ExecuteSqlAsync(SqlServerQuery query);
        /// <summary>
        /// Выполнить несколько запросов в одной транзакции
        /// </summary>
        /// <param name="queries">sql запросы</param>
        /// <returns>
        /// Возвращает true, если запросы успешно выполнены и транзакция завершена. 
        /// Возвращает false, если при выполнении запросов произошла ошибка и транзакция отменена.
        /// </returns>
        Task<bool> ExecuteSqlAsync(IEnumerable<SqlServerQuery> queries);
        /// <summary>
        /// Добавить объект в контекст
        /// </summary>
        /// <typeparam name="T">тип объекта</typeparam>
        /// <param name="entity">объект, который необходимо довабить</param>
        void Add<T>(T entity) where T : class;
        /// <summary>
        /// Загрузить данные
        /// </summary>
        /// <typeparam name="T">тип загружаемых данных (ассоциированный с таблицей базы данных)</typeparam>
        /// <returns>Возвращает список значений</returns>
        Task<List<T>> GetAsync<T>() where T : class;
        /// <summary>
        /// Сохранить изменения контекста в базе данных
        /// </summary>
        /// <returns>Возвращает количество строк, измененных в дазе данных</returns>
        Task<int> SaveAsync();
    }
}
