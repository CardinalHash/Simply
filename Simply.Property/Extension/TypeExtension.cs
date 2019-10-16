using System;

namespace Simply.Property
{
    public static class TypeExtension
    {
        private static string value(this Type t, object value, string delimiter)
        {
            if (value == null) return "null";
            if (t == typeof(string) || t == typeof(Guid) || t == typeof(Guid?) ||
                t == typeof(DateTime) || t == typeof(DateTime?))
                return $"{delimiter}{value}{delimiter}";
            if (t == typeof(bool) || t == typeof(bool?) ||
                t == typeof(int) || t == typeof(int?) ||
                t == typeof(long) || t == typeof(long?) ||
                t == typeof(decimal) || t == typeof(decimal?))
                return $"{value}";
            return null;
        }
        public static string databaseValue(this Type t, object value) => t.value(value, "'");
        public static T[] getAttributes<T>(this Type t) where T : class => Attribute.GetCustomAttributes(t, typeof(T)) as T[];
        public static T getAttribute<T>(this Type t) where T : class => Attribute.GetCustomAttribute(t, typeof(T)) as T;

        //public static object value(this Type t, string value)
        //{
        //    if (value == null) return null;
        //    if (t == typeof(Guid) || t == typeof(Guid?)) return new Guid(value);
        //    if (t == typeof(DateTime) || t == typeof(DateTime?)) return value.ToDate();
        //    if (t == typeof(bool) || t == typeof(bool?)) return value.ToBoolean();
        //    if (t == typeof(int) || t == typeof(int?)) return value.ToInt();
        //    if (t == typeof(long) || t == typeof(long?)) return value.ToLong();
        //    if (t == typeof(decimal) || t == typeof(decimal?)) return value.ToDecimal();
        //    return value;
        //}
        //public static string jsonValue(this Type t, object value) => t.value(value, "\"");
    }
}
