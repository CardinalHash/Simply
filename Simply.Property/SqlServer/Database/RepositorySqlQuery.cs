
namespace Simply.Property.SqlServer
{
    public class SqlQuery
    {
        public SqlQuery(string query)
        {
            Query = query;
        }
        public SqlQuery(string query, string json)
            : this(query)
        {
            Json = json;
        }
        public string Query { get; set; }
        public string Json { get; set; }
    }
}
