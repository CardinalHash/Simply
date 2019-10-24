using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

namespace Simply.Property.SqlServer
{
    internal sealed class QueryBuilder<T> : IQueryBuilder<T>
    {
        private AttributeType[] getAttributes<AttributeType>(Type t) where AttributeType : class => Attribute.GetCustomAttributes(t, typeof(AttributeType)) as AttributeType[];
        private AttributeType getAttribute<AttributeType>(Type t) where AttributeType : class => Attribute.GetCustomAttribute(t, typeof(AttributeType)) as AttributeType;
        private string getType(Property propertyInfo) => propertyTypeList.GetOrCreate(propertyInfo.Name, () => getDatabaseType(propertyInfo));
        private string getDatabaseType(Property propertyInfo)
        {
            if (Attribute.IsDefined(propertyInfo.PropertyInfo, typeof(SqlTypeAttribute))) 
                return getAttribute<SqlTypeAttribute>(propertyInfo.Type).Type;
            switch (propertyInfo.Type)
            {
                case Type t when t == typeof(string): return $"nvarchar({propertyInfo.MaxLength ?? 1024})";
                case Type t when t == typeof(DateTime) || t == typeof(DateTime?): return "datetime2(7)";
                case Type t when t == typeof(bool) || t == typeof(bool?): return "bit";
                case Type t when t == typeof(sbyte) || t == typeof(sbyte?): return "tinyint";
                case Type t when t == typeof(short) || t == typeof(short?): return "smallint";
                case Type t when t == typeof(int) || t == typeof(int?): return "int";
                case Type t when t == typeof(long) || t == typeof(long?): return "bigint";
                case Type t when t == typeof(decimal) || t == typeof(decimal?): return "decimal(18,2)";
                case Type t when t == typeof(float) || t == typeof(float?) || t == typeof(double) || t == typeof(double?): return "float";
                case Type t when t == typeof(Guid) || t == typeof(Guid?): return "uniqueidentifier";
                default: return null;
            }
        }
        private Property key;
        private SynchronizedCache<string, string> propertyTypeList;
        private ICollection<Property> toCreate, toUpdate, toInsert;
        private JsonSerializerSettings jsonSettingsForInsert;
        private JsonSerializerSettings jsonSettingsForUpdate;
        private JsonSerializerSettings jsonSettingsForDelete;
        private SynchronizedCache<string, JsonSerializerSettings> jsonSettingsForUpdateCacheList;
        private SynchronizedCache<string, JsonSerializerSettings> jsonSettingsForDeleteCacheList;
        private SynchronizedCache<string, string> updateCacheList;
        private SynchronizedCache<string, string> deleteCacheList;
        private readonly string insert, update, delete, createTable, truncateTable, dropTable;
        private readonly StringBuilder createNonClusteredIndexList;
        private NonClusteredIndexAttribute[] GetNonClusteredIndex() => getAttributes<NonClusteredIndexAttribute>(typeof(T));
        public QueryBuilder(IPropertyScope propertyScope)
        {
            // properties
            key = propertyScope.GetProperties<T>().FirstOrDefault(p => p.IsKey);
            propertyTypeList = new SynchronizedCache<string, string>();
            updateCacheList = new SynchronizedCache<string, string>();
            deleteCacheList = new SynchronizedCache<string, string>();
            var mappingProperties = propertyScope.GetProperties<T>().Where(p => p.NotMapped == false);
            toUpdate = mappingProperties.Where(p => p.IsKey == false).ToList();
            toInsert = mappingProperties.Where(p => p.IsIdentity == false).ToList();
            toCreate = mappingProperties.ToList();
            // json properties
            jsonSettingsForInsert = new JsonSerializerSettings { ContractResolver = (key.IsIdentity) ? (new JsonPropertySerializerContractResolver().IgnoreProperty(key)) : (new JsonPropertySerializerContractResolver()) };
            jsonSettingsForUpdate = new JsonSerializerSettings { ContractResolver = new JsonPropertySerializerContractResolver() };
            jsonSettingsForUpdateCacheList = new SynchronizedCache<string, JsonSerializerSettings>();
            jsonSettingsForDelete = new JsonSerializerSettings { ContractResolver = (new JsonPropertySerializerContractResolver().Property(key)) };
            jsonSettingsForDeleteCacheList = new SynchronizedCache<string, JsonSerializerSettings>();
            // sql queries
            createNonClusteredIndexList = new StringBuilder();
            GetNonClusteredIndex().ForEach(column => createNonClusteredIndexList.Append($"CREATE NONCLUSTERED INDEX [{column.Name}] ON [dbo].[{GetTable()}] ({string.Join(",", column.Properties.Select(p => $"[{p}] ASC"))}) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];"));
            createTable = $"IF OBJECT_ID('[dbo].[{GetTable()}]') IS NULL CREATE TABLE [{GetTable()}] ({String.Join(",", toCreate.Select(p => $"[{p.ColumnName}] {getType(p)} {(p.IsIdentity ? "IDENTITY(1,1)" : String.Empty)} {(p.IsKey || p.IsRequired ? "NOT NULL" : "NULL")}"))}" +
                $" CONSTRAINT [PK_{GetTable()}] PRIMARY KEY CLUSTERED ([{key.ColumnName}] ASC) WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY];";
            truncateTable = $"IF OBJECT_ID('[dbo].[{GetTable()}]') IS NOT NULL TRUNCATE TABLE [{GetTable()}]";
            dropTable = $"IF OBJECT_ID('[dbo].[{GetTable()}]') IS NOT NULL DROP TABLE [{GetTable()}];";
            insert = $"INSERT INTO [{GetTable()}] ({String.Join(",", toInsert.Select(p => $"[{p.ColumnName}]"))}) SELECT * FROM OPENJSON(@json) WITH ({string.Join(",", toInsert.Select(p => $"[{p.ColumnName}] {getType(p)} '$.{p.JsonProperty}'"))})";
            update = $"UPDATE [{GetTable()}] SET {string.Join(",", toUpdate.Select(p => $"[{p.ColumnName}]=source.{p.ColumnName}"))} FROM [{GetTable()}] original JOIN (SELECT * FROM OPENJSON(@json) WITH ({string.Join(",", toCreate.Select(p => $"[{p.ColumnName}] {getType(p)} '$.{p.JsonProperty}'"))})) source ON original.{key.ColumnName}=source.{key.ColumnName}";
            delete = $"DELETE original FROM [{GetTable()}] original JOIN (SELECT * FROM OPENJSON(@json) WITH ([{key.ColumnName}] {getType(key)} '$.{key.JsonProperty}')) source ON original.{key.ColumnName}=source.{key.ColumnName}";
        }
        public string GetTable() => getAttribute<TableAttribute>(typeof(T))?.Name;
        // Генерация запросов
        public JsonSerializerSettings JsonSettingsForInsert() => jsonSettingsForInsert;
        public JsonSerializerSettings JsonSettingsForUpdate() => jsonSettingsForUpdate;
        public JsonSerializerSettings JsonSettingsForUpdate(string[] properties) => jsonSettingsForUpdateCacheList.GetOrCreate(string.Join(",", properties), () => new JsonSerializerSettings { ContractResolver = (new JsonPropertySerializerContractResolver().Property(toUpdate.Where(p => Array.Exists(properties, u => u == p.ColumnName)).ToArray()).Property(key)) });
        public JsonSerializerSettings JsonSettingsForDelete() => jsonSettingsForDelete;
        public JsonSerializerSettings JsonSettingsForDelete(string[] properties) => jsonSettingsForDeleteCacheList.GetOrCreate(string.Join(",", properties), () => new JsonSerializerSettings { ContractResolver = (new JsonPropertySerializerContractResolver().Property(toCreate.Where(p => Array.Exists(properties, u => u == p.ColumnName)).ToArray())) });
        public string BuildCreateTable() => createTable;
        public string BuildTruncateTable() => truncateTable;
        public string BuildDropTable() => dropTable;
        public string BuildCreateNonClusteredIndexs() => createNonClusteredIndexList.ToString();
        public string BuildInsert() => insert;
        public string BuildUpdate() => update;
        public string BuildUpdate(string[] update) => updateCacheList.GetOrCreate(string.Join(",", update), () => $"UPDATE [{GetTable()}] SET {string.Join(",", toUpdate.Where(p => Array.Exists(update, u => u == p.ColumnName)).Select(p => $"[{p.ColumnName}]=source.{p.ColumnName}"))} FROM [{GetTable()}] original JOIN (SELECT * FROM OPENJSON(@json) WITH ({string.Join(",", toCreate.Where(p => p.IsKey || Array.Exists(update, u => u == p.ColumnName)).Select(p => $"[{p.ColumnName}] {getType(p)} '$.{p.JsonProperty}'"))})) source ON original.{key.ColumnName}=source.{key.ColumnName}");
        public string BuildDelete() => delete;
        public string BuildDelete(string[] delete) => deleteCacheList.GetOrCreate(string.Join(",", delete), () => $"DELETE original FROM [{GetTable()}] original JOIN (SELECT * FROM OPENJSON(@json) WITH ({string.Join(",", toCreate.Where(p => Array.Exists(delete, u => u == p.ColumnName)).Select(p => $"[{p.ColumnName}] {getType(p)} '$.{p.JsonProperty}'"))})) source ON {string.Join(" AND ", toCreate.Where(p => Array.Exists(delete, u => u == p.ColumnName)).Select(p => $"original.[{p.ColumnName}]=source.[{p.ColumnName}]"))}");
    }
}
