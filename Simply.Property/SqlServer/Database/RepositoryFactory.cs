namespace Simply.Property.SqlServer
{
    /// <summary>
    /// Обстрактный тип фабрики для создания IRepository
    /// Предоставляет доступ к базе данных через IQueryDatabase
    /// </summary>
    public abstract class RepositoryFactory : IRepositoryFactory
    {
        public RepositoryFactory() { Database = new QueryDatabase(this, new QueryScope(new PropertyScope())); }
        public abstract IRepository GetRepository();
        public IQueryDatabase Database { get; private set; }
    }
}
