using System.Collections.Concurrent;

namespace Simply.Property
{
    internal class PropertyScope : IPropertyScope
    {
        private ConcurrentDictionary<string, object> propertyScope = new ConcurrentDictionary<string, object>();
        public IPropertyManager<T> Property<T>() => (IPropertyManager<T>)propertyScope.GetOrAdd(typeof(IPropertyManager<T>).ToString(), new PropertyManager<T>());
    }
}
