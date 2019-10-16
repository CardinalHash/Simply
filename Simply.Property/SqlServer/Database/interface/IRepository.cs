using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simply.Property.SqlServer
{
    public interface IRepository : IDisposable
    {
        Task<int> ExecuteSqlAsync(string sql, string json = null);
        Task ExecuteSqlAsync(IEnumerable<SqlQuery> queries);
        Task<List<T>> GetAsync<T>() where T : class;
        Task<int> SaveAsync();
    }
}
