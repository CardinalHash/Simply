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
        public IQueryBuilder<T> Query<T>() => (IQueryBuilder<T>)queryScope.GetOrAdd(typeof(IQueryBuilder<T>).ToString(), new QueryBuilder<T>(scope.Property<T>()));
    }
}
