using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simply.Property.SqlServer
{
    public interface IRepositoryFactory
    {
        IRepository GetRepository();
        Task ExecuteSqlAsync(string sql);
        Task ExecuteSqlAsync(RepositorySqlQuery query);
        Task ExecuteSqlWithTransactionAsync(params RepositorySqlQuery[] queryList);
        Task CreateTableAsync<T>();
        Task TruncateTableAsync<T>();
        Task AddAsync<T>(IEnumerable<T> entites);
        Task AddWithTransactionAsync<T>(IEnumerable<T> objects);
        Task<int> BulkAddAsync<T>(IEnumerable<T> entites);
        Task UpdateAsync<T>(IEnumerable<T> entites, string[] properties);
        Task UpdateWithTransactionAsync<T>(IEnumerable<T> objects, string[] properties);
        Task<int> BulkUpdateAsync<T>(IEnumerable<T> entites, string[] properties);
        Task RemoveAsync<T>(IEnumerable<T> entites);
        Task RemoveWithTransactionAsync<T>(IEnumerable<T> objects);
        Task<int> BulkRemoveAsync<T>(IEnumerable<T> entites);
        RepositorySqlQuery CreateTableToSql<T>();
        RepositorySqlQuery AddToSql<T>(IEnumerable<T> entities);
        RepositorySqlQuery UpdateToSql<T>(IEnumerable<T> entities, string[] properties);
        RepositorySqlQuery RemoveToSql<T>(IEnumerable<T> entities);
    }
}
