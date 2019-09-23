
namespace Simply.Property.SqlServer
{
    public class RepositorySqlQuery
    {
        public RepositorySqlQuery(string query)
        {
            Query = query;
        }
        public RepositorySqlQuery(string query, string json)
            : this(query)
        {
            Json = json;
        }
        public string Query { get; set; }
        public string Json { get; set; }
    }
}
