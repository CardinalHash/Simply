using System.Collections.Concurrent;

namespace Simply.Property.SqlServer
{
    internal class QueryScope : IQueryScope
    {
        private ConcurrentDictionary<string, object> queryScope = new ConcurrentDictionary<string, object>();
        private readonly IPropertyScope scope;
        public QueryScope(IPropertyScope scope)
        {
            this.scope = scope;
        }
        public IQueryBuilder<T> query<T>() => (IQueryBuilder<T>)queryScope.GetOrAdd(typeof(IQueryBuilder<T>).ToString(), new QueryBuilder<T>(scope.property<T>()));
    }
}
