using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

namespace Simply.Property.SqlServer
{
    internal sealed class QueryBuilder<T> : IQueryBuilder<T>
    {
        private string getType(Property<T> propertyInfo) => propertyTypeList.GetOrAdd(propertyInfo, databaseType(propertyInfo));
        private string databaseType(Property<T> propertyInfo)
        {
            if (Attribute.IsDefined(propertyInfo.property, typeof(SqlTypeAttribute))) return propertyInfo.property.GetAttribute<SqlTypeAttribute>().Type;
            switch (propertyInfo.type)
            {
                case Type t when t == typeof(string): return $"nvarchar({propertyInfo.maxLength ?? 1024})";
                case Type t when t == typeof(DateTime) || t == typeof(DateTime?): return "datetime2(7)";
                case Type t when t == typeof(int) || t == typeof(int?): return "int";
                case Type t when t == typeof(bool) || t == typeof(bool?): return "bit";
                case Type t when t == typeof(long) || t == typeof(long?): return "bigint";
                case Type t when t == typeof(decimal) || t == typeof(decimal?): return "decimal(18,2)";
                case Type t when t == typeof(double) || t == typeof(double?): return "float";
                case Type t when t == typeof(Guid) || t == typeof(Guid?): return "uniqueidentifier";
                default: return null;
            }
        }
        private Property<T> key;
        private ConcurrentDictionary<Property<T>, string> propertyTypeList;
        private ICollection<Property<T>> toCreate, toUpdate, toInsert;
        private JsonSerializerSettings jsonSettingsForInsert;
        private JsonSerializerSettings jsonSettingsForUpdate;
        private JsonSerializerSettings jsonSettingsForDelete;
        private ConcurrentDictionary<string, JsonSerializerSettings> jsonSettingsForUpdateCacheList;
        private ConcurrentDictionary<string, JsonSerializerSettings> jsonSettingsForDeleteCacheList;
        private ConcurrentDictionary<string, string> updateCacheList;
        private ConcurrentDictionary<string, string> deleteCacheList;
        private readonly string insert, update, delete, createTable, truncateTable, dropTable;
        private readonly StringBuilder createNonClusteredIndexList;
        private NonClusteredIndexAttribute[] GetNonClusteredIndex() => typeof(T).getAttributes<NonClusteredIndexAttribute>();
        public QueryBuilder(IPropertyManager<T> propertyManager)
        {
            // properties
            key = propertyManager.FirstOrDefault(p => p.isKey);
            propertyTypeList = new ConcurrentDictionary<Property<T>, string>();
            updateCacheList = new ConcurrentDictionary<string, string>();
            deleteCacheList = new ConcurrentDictionary<string, string>();
            var mappingProperties = propertyManager.Where(p => p.jsonIgnore == false);
            toUpdate = mappingProperties.Where(p => p.isKey == false).ToList();
            toInsert = mappingProperties.Where(p => p.isIdentity == false).ToList();
            toCreate = mappingProperties.ToList();
            // json properties
            jsonSettingsForInsert = new JsonSerializerSettings { ContractResolver = (key.isIdentity) ? (new JsonPropertySerializerContractResolver().IgnoreProperty(key)) : (new JsonPropertySerializerContractResolver()) };
            jsonSettingsForUpdate = new JsonSerializerSettings { ContractResolver = new JsonPropertySerializerContractResolver() };
            jsonSettingsForUpdateCacheList = new ConcurrentDictionary<string, JsonSerializerSettings>();
            jsonSettingsForDelete = new JsonSerializerSettings { ContractResolver = (new JsonPropertySerializerContractResolver().Property(key)) };
            jsonSettingsForDeleteCacheList = new ConcurrentDictionary<string, JsonSerializerSettings>();
            // sql queries
            createNonClusteredIndexList = new StringBuilder();
            GetNonClusteredIndex().ForEach(column => createNonClusteredIndexList.Append($"CREATE NONCLUSTERED INDEX [{column.Name}] ON [dbo].[{GetTable()}] ({string.Join(",", column.Properties.Select(p => $"[{p}] ASC"))}) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];"));
            createTable = $"IF OBJECT_ID('[dbo].[{GetTable()}]') IS NULL CREATE TABLE [{GetTable()}] ({String.Join(",", toCreate.Select(p => $"[{p.column}] {getType(p)} {(p.isIdentity ? "IDENTITY(1,1)" : String.Empty)} {(p.isKey || p.isRequired ? "NOT NULL" : "NULL")}"))}" +
                $" CONSTRAINT [PK_{GetTable()}] PRIMARY KEY CLUSTERED ([{key.column}] ASC) WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY];";
            truncateTable = $"IF OBJECT_ID('[dbo].[{GetTable()}]') IS NOT NULL TRUNCATE TABLE [{GetTable()}]";
            dropTable = $"IF OBJECT_ID('[dbo].[{GetTable()}]') IS NOT NULL DROP TABLE [{GetTable()}];";
            insert = $"INSERT INTO [{GetTable()}] ({String.Join(",", toInsert.Select(p => $"[{p.column}]"))}) SELECT * FROM OPENJSON(@json) WITH ({string.Join(",", toInsert.Select(p => $"[{p.column}] {getType(p)} '$.{p.jsonProperty}'"))})";
            update = $"UPDATE [{GetTable()}] SET {string.Join(",", toUpdate.Select((Property<T> p) => $"[{p.column}]=source.{p.column}"))} FROM [{GetTable()}] original JOIN (SELECT * FROM OPENJSON(@json) WITH ({string.Join(",", toCreate.Select((Property<T> p) => $"[{p.column}] {getType(p)} '$.{p.jsonProperty}'"))})) source ON original.{key.column}=source.{key.column}";
            delete = $"DELETE original FROM [{GetTable()}] original JOIN (SELECT * FROM OPENJSON(@json) WITH ([{key.column}] {getType(key)} '$.{key.jsonProperty}')) source ON original.{key.column}=source.{key.column}";
        }
        public string GetTable() => typeof(T).getAttribute<TableAttribute>()?.Name;
        // Генерация запросов
        public JsonSerializerSettings JsonSettingsForInsert() => jsonSettingsForInsert;
        public JsonSerializerSettings JsonSettingsForUpdate() => jsonSettingsForUpdate;
        public JsonSerializerSettings JsonSettingsForUpdate(string[] properties) => jsonSettingsForUpdateCacheList.GetOrAdd(string.Join(",", properties), new JsonSerializerSettings { ContractResolver = (new JsonPropertySerializerContractResolver().Property(toUpdate.Where((Property<T> p) => Array.Exists(properties, u => u == p.column)).Select((Property<T> p) => p).ToArray()).Property(key)) });
        public JsonSerializerSettings JsonSettingsForDelete() => jsonSettingsForDelete;
        public JsonSerializerSettings JsonSettingsForDelete(string[] properties) => jsonSettingsForDeleteCacheList.GetOrAdd(string.Join(",", properties), new JsonSerializerSettings { ContractResolver = (new JsonPropertySerializerContractResolver().Property(toCreate.Where((Property<T> p) => Array.Exists(properties, u => u == p.column)).Select((Property<T> p) => p).ToArray())) });
        public string BuildCreateTable() => createTable;
        public string BuildTruncateTable() => truncateTable;
        public string BuildDropTable() => dropTable;
        public string BuildCreateNonClusteredIndexs() => createNonClusteredIndexList.ToString();
        public string BuildInsert() => insert;
        public string BuildUpdate() => update;
        public string BuildUpdate(string[] update) => updateCacheList.GetOrAdd(string.Join(",", update), $"UPDATE [{GetTable()}] SET {string.Join(",", toUpdate.Where((Property<T> p) => Array.Exists(update, u => u == p.column)).Select((Property<T> p) => $"[{p.column}]=source.{p.column}"))} FROM [{GetTable()}] original JOIN (SELECT * FROM OPENJSON(@json) WITH ({string.Join(",", toCreate.Where((Property<T> p) => p.isKey || Array.Exists(update, u => u == p.column)).Select((Property<T> p) => $"[{p.column}] {getType(p)} '$.{p.jsonProperty}'"))})) source ON original.{key.column}=source.{key.column}");
        public string BuildDelete() => delete;
        public string BuildDelete(string[] delete) => deleteCacheList.GetOrAdd(string.Join(",", delete), $"DELETE original FROM [{GetTable()}] original JOIN (SELECT * FROM OPENJSON(@json) WITH ({string.Join(",", toCreate.Where(p => Array.Exists(delete, u => u == p.column)).Select((Property<T> p) => $"[{p.column}] {getType(p)} '$.{p.jsonProperty}'"))})) source ON {string.Join(" AND ", toCreate.Where(p => Array.Exists(delete, u => u == p.column)).Select((Property<T> p) => $"original.[{p.column}]=source.[{p.column}]"))}");

        //public string BuildInsert(T value) => $"INSERT INTO [{GetTable()}] ({String.Join(",", toInsert.Select(p => $"[{p.column}]"))}) VALUES ({String.Join(",", toInsert.Select(p => $"{p.type.databaseValue(p.getter(value))}"))})";
        //public string BuildUpdate(T value) => $"UPDATE [{GetTable()}] SET {String.Join(",", toUpdate.Select(p => $"[{p.column}]='{p.getter(value)}'"))} WHERE {key.column}={key.type.databaseValue(key.getter(value))}";
        //public string BuildUpdate(T value, string[] update) => $"UPDATE [{GetTable()}] SET {String.Join(",", toUpdate.Where(p => Array.Exists(update, u => u == p.column)).Select(p => $"[{p.column}]={p.type.databaseValue(p.getter(value))}"))} WHERE {key.column}={key.type.databaseValue(key.getter(value))}";
        //public string BuildDelete(T value) => $"DELETE FROM [{GetTable()}] WHERE {key.column}={key.type.databaseValue(key.getter(value))}";
        //public string BuildDelete(T value, string[] where) => $"DELETE FROM [{GetTable()}] WHERE {String.Join(" AND ", toUpdate.Where(p => Array.Exists(where, u => u == p.name)).Select(p => $"[{p.column}]={p.type.databaseValue(p.getter(value))}"))}";
    }
}
