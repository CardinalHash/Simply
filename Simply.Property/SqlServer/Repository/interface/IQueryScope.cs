
namespace Simply.Property.SqlServer
{
    public interface IQueryScope
    {
        IQueryBuilder<T> query<T>();
    }
}
