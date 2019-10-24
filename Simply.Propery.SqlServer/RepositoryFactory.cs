namespace Simply.Property.SqlServer
{
    /// <summary>
    /// Обстрактный тип фабрики для создания экземпляров IRepository
    /// Предоставляет прямой доступ к базе данных через свойство Database
    /// </summary>
    public abstract class RepositoryFactory : IRepositoryFactory
    {
        /// <summary>
        /// Конструктор фабрики репоситориев
        /// </summary>
        public RepositoryFactory() { Database = new QueryDatabase(this, new QueryScope(new PropertyScope())); }
        /// <summary>
        /// Получить экземпляр IRepository
        /// </summary>
        /// <returns></returns>
        public abstract IRepository GetRepository();
        /// <summary>
        /// Прямой дуступ к базе данных (IQueryDatabase)
        /// </summary>
        public IQueryDatabase Database { get; private set; }
    }
}
