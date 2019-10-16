namespace Simply.Property.SqlServer
{
    public interface IRepositoryFactory
    {
        IRepository GetRepository();
        IQueryDatabase Database { get; }
    }
}
