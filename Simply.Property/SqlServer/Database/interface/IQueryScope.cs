
namespace Simply.Property.SqlServer
{
    internal interface IQueryScope
    {
        IQueryBuilder<T> query<T>();
    }
}
