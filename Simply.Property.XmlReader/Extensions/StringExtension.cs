using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Simply.Property
{
    internal static class StringExtension
    {
        static CultureInfo enUS = new CultureInfo("en-US");
        public static string ToValue(this string value, Type type)
        {
            // replace hidden symbol
            value = value.Replace("\t", "").Replace("\n", "");
            // check DateTime in string
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                if (value.ToDateTime(out DateTime result))
                    return $"\"{result:s}\"";
                else
                    return "null";
            }
            else
            // other String
            {
                return $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\""; 
            }
        }
        public static bool ToDateTime(this string value, out DateTime result)
        {
            if (DateTime.TryParseExact(value, "yyyyMMdd", enUS, DateTimeStyles.None, out result))
                return true;
            if (DateTime.TryParse(value, out result))
                return true;
            return false;
        }
    }
}
