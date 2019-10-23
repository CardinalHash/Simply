using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Simply.Property
{
    internal static class StringExtension
    {
        public static string ToXmlValue(this string value, StringBuilder container)
        {
            container.Clear();
            if (Regex.IsMatch(value, "^((0[1-9]|[12][0-9]|3[01])[- /.](0[1-9]|1[012])[- /.](19|20)\\d\\d)|((19|20)\\d\\d(0[1-9]|1[012])(0[1-9]|[12][0-9]|3[01]))$"))
                container.Append(value.ToDate()?.ToString("MM/dd/yyyy"));
            else
                container.Append(value);
            return container.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\t", "").Replace("\n", "").ToString();
        }
        private static DateTime? ToDate(this string value)
        {
            if (DateTime.TryParse(value, out DateTime result) || value.Length == 8 && DateTime.TryParse(value.Substring(6, 2) + "." + value.Substring(4, 2) + "." + value.Substring(0, 4), out result))
                return result;
            return null;
        }
    }
}
