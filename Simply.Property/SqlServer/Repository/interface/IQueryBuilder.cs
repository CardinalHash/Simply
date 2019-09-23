using Newtonsoft.Json;

namespace Simply.Property.SqlServer
{
    public interface IQueryBuilder<T>
    {
        string GetTable();
        JsonSerializerSettings JsonSettingsForRemove();
        JsonSerializerSettings JsonSettingsForInsert();
        JsonSerializerSettings JsonSettingsForUpdate(string[] properties);
        string BuildCreateTable();
        string BuildTruncate();
        string BuildDropTable();
        string BuildCreateNonClusteredIndexs();
        string BuildInsert(T value);
        string BuildInsert();
        string BuildUpdate(T value);
        string BuildUpdate(string[] update);
        string BuildUpdate(T value, string[] update);
        string BuildDelete(T value);
        string BuildDelete(T value, string[] where);
        string BuildDelete();
    }
}
