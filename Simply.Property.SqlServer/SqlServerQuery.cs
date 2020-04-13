using System;

namespace Simply.Property.SqlServer
{
    /// <summary>
    /// Класс для хранения sql-запрос и jsonData параметра
    /// Если значение параметра jsonData=null, то он должен игнорироваться
    /// </summary>
    public class SqlServerQuery
    {
        /// <summary>
        /// Конструктор класса для создания sql-зароса
        /// </summary>
        /// <param name="query">sql-запрос</param>
        public SqlServerQuery(string query)
        {
            Query = query ?? throw new ArgumentNullException("query");
        }
        /// <summary>
        /// Конструктор класса для создания sql зароса
        /// </summary>
        /// <param name="query">sql-запрос</param>
        /// <param name="jsonData">данные запроса в формате json</param>
        public SqlServerQuery(string query, string jsonData)
            : this(query)
        {
            Query = query;
            JsonData = jsonData;
        }
        /// <summary>
        /// Sql-запрос
        /// </summary>
        public string Query { private set; get; }
        /// <summary>
        /// данные запроса в формате json
        /// </summary>
        public string JsonData { private set; get; }
        /// <summary>
        /// название параметра sql-запроса
        /// </summary>
        public string JsonParameter => "@json";
    }
}
