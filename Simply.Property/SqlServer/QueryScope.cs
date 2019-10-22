namespace Simply.Property.SqlServer
{
    internal class QueryScope : IQueryScope
    {
        private SynchronizedCache<string, object> queryScope = new SynchronizedCache<string, object>();
        private readonly IPropertyScope scope;
        public QueryScope(IPropertyScope scope)
        {
            this.scope = scope;
        }
        public IQueryBuilder<T> Query<T>() => (IQueryBuilder<T>)queryScope.GetOrCreate(typeof(IQueryBuilder<T>).ToString(), () => new QueryBuilder<T>(scope.Property<T>()));
    }
}
