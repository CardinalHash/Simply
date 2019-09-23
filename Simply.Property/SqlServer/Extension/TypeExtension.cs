using System;

namespace Simply.Property.SqlServer
{
    public static class TypeExtension
    {
        public static string sqlPropertyType(this Type t, int? maxLength = null)
        {
            if (t == typeof(string)) return $"nvarchar({maxLength ?? 1024})";
            if (t == typeof(DateTime) || t == typeof(DateTime?)) return "datetime2(7)";
            if (t == typeof(int) || t == typeof(int?)) return "int";
            if (t == typeof(bool) || t == typeof(bool?)) return "bit";
            if (t == typeof(long) || t == typeof(long?)) return "bigint";
            if (t == typeof(decimal) || t == typeof(decimal?)) return "decimal(18,2)";
            if (t == typeof(Guid) || t == typeof(Guid?)) return "uniqueidentifier";
            return null;
        }
    }
}
