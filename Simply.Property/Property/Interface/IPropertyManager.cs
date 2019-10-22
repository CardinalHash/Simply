using System.Collections.Generic;

namespace Simply.Property
{
    internal interface IPropertyManager<T> : IEnumerable<Property<T>>
    {
        Property<T> Get(string property);
        bool Contains(string property);
    }
}
