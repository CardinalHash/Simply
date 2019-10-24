﻿
namespace Simply.Property.SqlServer
{
    /// <summary>
    /// Класс для хранения sql-запрос и json параметра
    /// Если значение параметра json=null, то он должен игнорироваться
    /// </summary>
    public class SqlServerQuery
    {
        /// <summary>
        /// Конструктор класса для создания sql-зароса
        /// </summary>
        /// <param name="query">sql-запрос</param>
        public SqlServerQuery(string query)
        {
            Query = query;
        }
        /// <summary>
        /// Конструктор класса для создания sql зароса
        /// </summary>
        /// <param name="query">sql-запрос</param>
        /// <param name="json">json параметр</param>
        public SqlServerQuery(string query, string json)
            : this(query)
        {
            Json = json;
        }
        /// <summary>
        /// sql-запрос
        /// </summary>
        public string Query { get; set; }
        /// <summary>
        /// параметр json
        /// </summary>
        public string Json { get; set; }
    }
}