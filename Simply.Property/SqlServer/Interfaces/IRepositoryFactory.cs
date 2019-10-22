namespace Simply.Property.SqlServer
{
    /// <summary>
    /// Интерфейс фабрики для создания экземпляров IRepository
    /// Предоставляет прямой доступ к базе данных через свойство Database
    /// </summary>
    public interface IRepositoryFactory
    {
        /// <summary>
        /// Получить экземпляр IRepository
        /// </summary>
        /// <returns></returns>
        IRepository GetRepository();
        /// <summary>
        /// Прямой дуступ к базе данных (IQueryDatabase)
        /// </summary>
        IQueryDatabase Database { get; }
    }
}
