using System.Collections.Generic;

namespace Simply.Property
{
    public interface IPropertyManager<T> : IEnumerable<Property<T>>
    {
        Property<T> get(string property);
        bool contains(string property);
    }
}
