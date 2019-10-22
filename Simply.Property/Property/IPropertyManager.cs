using System;
using System.Collections.Generic;

namespace Simply.Property
{
    internal interface IPropertyManager<T> : IEnumerable<Property>
    {
        Property Get(string property);
        bool Contains(string property);
    }
}
