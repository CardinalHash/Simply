using System;
using System.Collections.Generic;

namespace Simply.Property
{
    public static class EnumerableExtension
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> entities, Action<T> a)
        {
            foreach (var e in entities) a(e);
            return entities;
        }
    }
}
