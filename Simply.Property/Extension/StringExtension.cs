using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Simply.Property
{
    public static class StringExtension
    {
        public static string ToXmlValue(this string value, StringBuilder container)
        {
            container.Clear();
            if (Regex.IsMatch(value, "^(0[1-9]|[12][0-9]|3[01])[- /.](0[1-9]|1[012])[- /.](19|20)\\d\\d$"))
                container.Append(DateTime.Parse(value).ToString("MM/dd/yyyy")); else container.Append(value);
            return container.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\t", "").Replace("\n", "").ToString();
        }
        public static DateTime? ToDate(this string value)
        {
            if (DateTime.TryParse(value, out DateTime result) ||
                value.Length == 8 && DateTime.TryParse(value.Substring(6, 2) + "." + value.Substring(4, 2) + "." + value.Substring(0, 4), out result))
            {
                return result;
            }
            return null;
        }
        public static int? ToInt(this string value)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return null;
        }
        public static bool? ToBoolean(this string value)
        {
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }
            return null;
        }
        public static long? ToLong(this string value)
        {
            if (long.TryParse(value, out long result))
            {
                return result;
            }
            return null;
        }
        public static decimal? ToDecimal(this string value)
        {
            if (decimal.TryParse(value, out decimal result))
            {
                return result;
            }
            return null;
        }
    }
}
