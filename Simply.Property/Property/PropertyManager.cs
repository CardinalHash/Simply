using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Collections;
using System;

namespace Simply.Property
{
    internal class PropertyManager<T> : IPropertyManager<T>
    {
        private readonly Dictionary<string, Property> properties;
        private readonly List<Property> propertyList = new List<Property>();
        public PropertyManager()
        {
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                if (propertyInfo.GetAttribute<InversePropertyAttribute>() == null)
                {
                    var property = new Property(propertyInfo);
                    if (property.IsKey) propertyList.Insert(0, property); else propertyList.Add(property);
                }
            }
            properties = propertyList.ToDictionary(p => p.Name);
        }
        public bool Contains(string property) => properties.ContainsKey(property);
        public Property Get(string property) => Contains(property) ? properties[property] : null;
        public IEnumerator<Property> GetEnumerator() => propertyList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => propertyList.GetEnumerator();
    }
}
