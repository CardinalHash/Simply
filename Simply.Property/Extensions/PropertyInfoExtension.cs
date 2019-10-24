using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FastExpressionCompiler;

namespace Simply.Property
{
    internal static class PropertyInfoExtension
    {
        public static T GetAttribute<T>(this PropertyInfo property) where T: class => property.GetCustomAttributes(true).FirstOrDefault(a => a is T) as T;
        public static Func<T, object> GetValueGetter<T>(this PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
            var property = Expression.Property(instance, propertyInfo);
            var convert = Expression.TypeAs(property, typeof(object));
            return (Func<T, object>)Expression.Lambda(convert, instance).CompileFast();
        }
        public static Action<T, object> GetValueSetter<T>(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetSetMethod() != null)
            {
                var instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
                var argument = Expression.Parameter(typeof(object), "a");
                var setterCall = Expression.Call(instance, propertyInfo.GetSetMethod(), Expression.Convert(argument, propertyInfo.PropertyType));
                return (Action<T, object>)Expression.Lambda(setterCall, instance, argument).CompileFast();
            }
            else
            {
                return null;
            }
        }
    }
}
