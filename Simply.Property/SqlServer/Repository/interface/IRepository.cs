using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simply.Property.SqlServer
{
    public interface IRepository : IDisposable
    {
        Task<int> ExecuteSqlAsync(string sql);
        Task<int> ExecuteSqlAsync(RepositorySqlQuery query);
        Task<int> ExecuteSqlWithJsonAsync(string sql, string json);
        Task ExecuteSqlWithTransactionAsync(IEnumerable<RepositorySqlQuery> queries);
        Task<List<T>> GetAsync<T>() where T : class;
        Task<int> AddManyAsync<T>(IEnumerable<T> entities) where T : class;
        Task<int> AddAsync<T>(T entity) where T : class;
        Task<int> SaveAsync();
    }
}
