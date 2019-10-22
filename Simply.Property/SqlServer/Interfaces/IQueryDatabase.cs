using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simply.Property.SqlServer
{
    /// <summary>
    /// Интерфейс для работы с базой данных
    /// </summary>
    public interface IQueryDatabase
    {
        
        /// <summary>
        /// Выполнить несколько SQL-запросов в одной транзакции
        /// </summary>
        /// <param name="queryList">Список запросов</param>
        /// <returns></returns>
        Task ExecuteSqlAsync(params SqlQuery[] queryList);
        /// <summary>
        /// Выполнить одиночный SQL-запрос
        /// </summary>
        /// <param name="query">SQL-запрос для выполнения</param>
        /// <returns></returns>
        Task ExecuteSqlAsync(SqlQuery query);

        /// <summary>
        /// Создать Sql-запрос
        /// </summary>
        /// <param name="query">Содержание Sql-запроса</param>
        /// <param name="json">Параметры запроса</param>
        /// <returns></returns>
        SqlQuery CreateSqlQuery(string query, string json = null);

        /// <summary>
        /// Создать SQL-запрос для создания таблицы в базе данных
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается таблица</typeparam>
        /// <returns></returns>
        SqlQuery CreateTableToSql<T>();
        /// <summary>
        /// Создать SQL-запрос для удаления данных из таблицы
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается таблица</typeparam>
        /// <returns></returns>
        SqlQuery TruncateTableToSql<T>();
        /// <summary>
        /// Создать SQL-запрос для удаления таблицы из базы данных
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается таблица</typeparam>
        /// <returns></returns>
        SqlQuery DropTableToSql<T>();

        /// <summary>
        /// (single SQL-query) Создать SQL-запрос для вставки данных
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="json">Сериализованные объекты для вставки в виде json-строки</param>
        /// <returns></returns>
        SqlQuery AddToSql<T>(string json);
        /// <summary>
        /// (single SQL-query) Создать SQL-запрос для вставки данных
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="entities">Объекты для вставки</param>
        /// <returns></returns>
        SqlQuery AddToSql<T>(IEnumerable<T> entities);
        
        /// <summary>
        /// (single SQL-query) Создать SQL-запрос для обновления данных (обновляем все поля)
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="entities">Объекты для обновления</param>
        /// <returns></returns>
        SqlQuery UpdateToSql<T>(IEnumerable<T> entities);
        /// <summary>
        /// (single SQL-query) Создать SQL-запрос для обновления данных (обновляем только указанные поля)
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="entities">Объекты для обновления</param>
        /// <param name="properties">Поля таблицы, которые необходимо обновить</param>
        /// <returns></returns>
        SqlQuery UpdateToSql<T>(IEnumerable<T> entities, string[] properties);
        
        /// <summary>
        /// (single SQL-query) Создать SQL-запрос для удаления данных (удаляем данные по ключу)
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="entities">Объекты для удаления</param>
        /// <returns></returns>
        SqlQuery RemoveToSql<T>(IEnumerable<T> entities);
        /// <summary>
        /// (single SQL-query) Создать SQL-запрос для удаления данных (удаляем данные по равенству указанных полей)
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="entities">Объекты с данными для формирования запроса</param>
        /// <param name="properties">Поля таблицы для сравнения</param>
        /// <returns></returns>
        SqlQuery RemoveToSql<T>(IEnumerable<T> entities, string[] properties);

        /// <summary>
        /// Создать таблицу
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <returns></returns>
        Task CreateTableAsync<T>();
        /// <summary>
        /// Удалить данные из таблицы
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <returns></returns>
        Task TruncateTableAsync<T>();
        /// <summary>
        /// Удалить таблицу
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <returns></returns>
        Task DropTableAsync<T>();

        /// <summary>
        /// (single SQL-query) Выполнить вставку данных
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="json">Сериализованные объекты для вставки в виде json-строки</param>
        /// <returns></returns>
        Task AddAsync<T>(string json);
        /// <summary>
        /// (single SQL-query) Выполнить вставку данных
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="objects">Объекты для вставки</param>
        /// <returns></returns>
        Task AddAsync<T>(IEnumerable<T> objects);
        /// <summary>
        /// (single SQL-query) Выполнить обновление данных (обновляем все поля таблицы)
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="objects">Объекты для обновления</param>
        /// <returns></returns>
        Task UpdateAsync<T>(IEnumerable<T> objects);
        /// <summary>
        /// (single SQL-query) Выполнить обновление данных (обновляем только выбранные поля)
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="objects">Объекты для обновления</param>
        /// <param name="properties">Поля таблицы, которые необходимо обновить</param>
        /// <returns></returns>
        Task UpdateAsync<T>(IEnumerable<T> objects, string[] properties);
        /// <summary>
        /// (single SQL-query) Выполнить удаление данных (удаляем записи по ключевому полю)
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="objects">Объекты для удаления</param>
        /// <returns></returns>
        Task RemoveAsync<T>(IEnumerable<T> objects);
        /// <summary>
        /// (single SQL-query) Выполнить SQL-запрос для удаления данных по равенству полей
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="objects">Объекты с данными для формирования запроса</param>
        /// <param name="properties">Поля таблицы для сравнения</param>
        /// <returns></returns>
        Task RemoveAsync<T>(IEnumerable<T> objects, string[] properties);

        /// <summary>
        /// (many SQL-queries) Выполнить вставку данных блоками
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="objects">Объекты для вставки</param>
        /// <returns></returns>
        Task<int> BulkAddAsync<T>(IEnumerable<T> objects);
        /// <summary>
        /// (many SQL-queries) Выполнить обновление блоками всех полей таблицы
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="objects">Объекты для обновления</param>
        /// <returns></returns>
        Task<int> BulkUpdateAsync<T>(IEnumerable<T> objects);
        /// <summary>
        /// (many SQL-queries) Выполнить обновление блоками, указанных полей таблицы
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="objects">Объекты для обновления</param>
        /// <param name="properties">Поля таблицы, которые необходимо обновить</param>
        /// <returns></returns>
        Task<int> BulkUpdateAsync<T>(IEnumerable<T> objects, string[] properties);
        /// <summary>
        /// (many SQL-queries) Выполнить удаление записей блоками
        /// </summary>
        /// <typeparam name="T">Тип данных на основе которого создается SQL запрос</typeparam>
        /// <param name="objects">Объекты для удаления</param>
        /// <returns></returns>
        Task<int> BulkRemoveAsync<T>(IEnumerable<T> objects);
    }
}
