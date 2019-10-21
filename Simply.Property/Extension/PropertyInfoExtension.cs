using System.Linq;
using System.Reflection;

namespace Simply.Property
{
    internal static class PropertyInfoExtension
    {
        public static T GetAttribute<T>(this PropertyInfo property) where T: class => property.GetCustomAttributes(true).FirstOrDefault(a => a is T) as T;
    }
}
