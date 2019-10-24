using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Simply.Property
{
    internal static class StringExtension
    {
        public static string ToValue(this string value, StringBuilder container)
        {
            container.Clear();
            if (Regex.IsMatch(value, "^((0[1-9]|[12][0-9]|3[01])[- /.](0[1-9]|1[012])[- /.](19|20)\\d\\d)|((19|20)\\d\\d(0[1-9]|1[012])(0[1-9]|[12][0-9]|3[01]))$"))
                container.Append(value.ToDate()?.ToString("s"));
            else
                container.Append(value);
            return container.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\t", "").Replace("\n", "").ToString();
        }
        public static DateTime? ToDate(this string value)
        {
            if (DateTime.TryParse(value, out DateTime result))
                return result;
            return null;
        }
    }
}
