using System.Collections.Concurrent;

namespace Simply.Property
{
    public class PropertyScope : IPropertyScope
    {
        private ConcurrentDictionary<string, object> propertyScope = new ConcurrentDictionary<string, object>();
        public IPropertyManager<T> property<T>() => (IPropertyManager<T>)propertyScope.GetOrAdd(typeof(IPropertyManager<T>).ToString(), new PropertyManager<T>());
        public void setter<T>(string property, T item, object value) => property<T>().get(property)?.setter?.Invoke(item, value);
        public object getter<T>(string property, T item) => property<T>().get(property)?.getter(item);
    }
}
