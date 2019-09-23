using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Simply.Property.SqlServer
{
    public sealed class QueryBuilder<T> : IQueryBuilder<T>
    {
        private Property<T> key;
        private ICollection<Property<T>> toCreate, toUpdate, toInsert;
        private JsonSerializerSettings jsonSettingsForRemove;
        private JsonSerializerSettings jsonSettingsForInsert;
        private ConcurrentDictionary<string[], JsonSerializerSettings> jsonSettingsForUpdate;
        private readonly string insert, delete, createTable, truncateTable, dropTable;
        private ConcurrentDictionary<string[], string> update;
        private readonly StringBuilder createNonClusteredIndexList;
        private NonClusteredIndexAttribute[] GetNonClusteredIndex() => typeof(T).getAttributes<NonClusteredIndexAttribute>();
        public QueryBuilder(IPropertyManager<T> propertyManager)
        {
            // properties
            key = propertyManager.FirstOrDefault(p => p.isKey);
            var mappingProperties = propertyManager.Where(p => p.jsonIgnore == false);
            toUpdate = mappingProperties.Where(p => p.isKey == false).ToList();
            toInsert = mappingProperties.Where(p => p.isIdentity == false).ToList();
            toCreate = mappingProperties.ToList();
            // json properties
            jsonSettingsForRemove = new JsonSerializerSettings { ContractResolver = (new JsonPropertySerializerContractResolver().Property(key)) };
            jsonSettingsForInsert = new JsonSerializerSettings { ContractResolver = (key.isIdentity) ? (new JsonPropertySerializerContractResolver().IgnoreProperty(key)) : (new JsonPropertySerializerContractResolver()) };
            jsonSettingsForUpdate = new ConcurrentDictionary<string[], JsonSerializerSettings>();
            // sql queries
            createNonClusteredIndexList = new StringBuilder();
            GetNonClusteredIndex().ForEach(column => createNonClusteredIndexList.Append($"CREATE NONCLUSTERED INDEX [{column.Name}] ON [dbo].[{GetTable()}] ({String.Join(",", column.Properties.Select(p => $"[{p}] ASC"))}) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];"));
            createTable = $"IF OBJECT_ID('[dbo].[{GetTable()}]') IS NULL CREATE TABLE [{GetTable()}] ({String.Join(",", toCreate.Select(p => $"[{p.column}] {p.type.sqlPropertyType(p.maxLength)} {(p.isIdentity ? "IDENTITY(1,1)" : String.Empty)} {(p.isKey ? "NOT NULL" : "NULL")}"))}" +
                $" CONSTRAINT [PK_{GetTable()}] PRIMARY KEY CLUSTERED ([{key.column}] ASC) WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY];";
            truncateTable = $"IF OBJECT_ID('[dbo].[{GetTable()}]') IS NOT NULL TRUNCATE TABLE [{GetTable()}]";
            dropTable = $"IF OBJECT_ID('[dbo].[{GetTable()}]') IS NOT NULL DROP TABLE [{GetTable()}];";
            insert = $"INSERT INTO [{GetTable()}] ({String.Join(",", toInsert.Select(p => $"[{p.column}]"))}) SELECT * FROM OPENJSON(@json) WITH ({String.Join(",", toInsert.Select(p => $"[{p.column}] {p.type.sqlPropertyType(p.maxLength)} '$.{p.jsonProperty}'"))})";
            update = new ConcurrentDictionary<string[], string>();
            delete = $"DELETE original FROM [{GetTable()}] original JOIN (SELECT * FROM OPENJSON(@json) WITH ([{key.column}] {key.type.sqlPropertyType(key.maxLength)} '$.{key.jsonProperty}')) source ON original.{key.column}=source.{key.column}";
        }
        public string GetTable() => typeof(T).getAttribute<TableAttribute>()?.Name;
        // Генерация запросов
        public JsonSerializerSettings JsonSettingsForInsert() => jsonSettingsForInsert;
        public JsonSerializerSettings JsonSettingsForRemove() => jsonSettingsForRemove;
        public JsonSerializerSettings JsonSettingsForUpdate(string[] properties) => jsonSettingsForUpdate.GetOrAdd(properties, new JsonSerializerSettings { ContractResolver = (new JsonPropertySerializerContractResolver().Property(toUpdate.Where((Property<T> p) => Array.Exists(properties, u => u == p.column)).Select((Property<T> p) => p).ToArray()).Property(key)) });
        public string BuildCreateTable() => createTable;
        public string BuildTruncate() => truncateTable;
        public string BuildDropTable() => dropTable;
        public string BuildCreateNonClusteredIndexs() => createNonClusteredIndexList.ToString();
        public string BuildInsert(T value) => $"INSERT INTO [{GetTable()}] ({String.Join(",", toInsert.Select(p => $"[{p.column}]"))}) VALUES ({String.Join(",", toInsert.Select(p => $"{p.type.sqlValue(p.getter(value))}"))})";
        public string BuildInsert() => insert;
        public string BuildUpdate(T value) => $"UPDATE [{GetTable()}] SET {String.Join(",", toUpdate.Select(p => $"[{p.column}]='{p.getter(value)}'"))} WHERE {key.column}={key.type.sqlValue(key.getter(value))}";
        public string BuildUpdate(T value, string[] update) => $"UPDATE [{GetTable()}] SET {String.Join(",", toUpdate.Where(p => Array.Exists(update, u => u == p.column)).Select(p => $"[{p.column}]={p.type.sqlValue(p.getter(value))}"))} WHERE {key.column}={key.type.sqlValue(key.getter(value))}";
        public string BuildUpdate(string[] update) => this.update.GetOrAdd(update, $"UPDATE [{GetTable()}] SET {string.Join(",", toUpdate.Where((Property<T> p) => Array.Exists(update, u => u == p.column)).Select((Property<T> p) => $"[{p.column}]=source.{p.column}"))} FROM [{GetTable()}] original JOIN (SELECT * FROM OPENJSON(@json) WITH ({string.Join(",", toCreate.Where((Property<T> p) => p.isKey || Array.Exists(update, u => u == p.column)).Select((Property<T> p) => $"[{p.column}] {p.type.sqlPropertyType(p.maxLength)} '$.{p.jsonProperty}'"))})) source ON original.{key.column}=source.{key.column}");
        public string BuildDelete(T value) => $"DELETE FROM [{GetTable()}] WHERE {key.column}={key.type.sqlValue(key.getter(value))}";
        public string BuildDelete(T value, string[] where) => $"DELETE FROM [{GetTable()}] WHERE {String.Join(" AND ", toUpdate.Where(p => Array.Exists(where, u => u == p.name)).Select(p => $"[{p.column}]={p.type.sqlValue(p.getter(value))}"))}";
        public string BuildDelete() => delete;
    }
}
