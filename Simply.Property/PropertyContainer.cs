using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Simply.Property
{
    internal class PropertyContainer<T>
    {
        private readonly Dictionary<string, Property> properties;
        private readonly Dictionary<string, Func<T, object>> getter = new Dictionary<string, Func<T, object>>();
        private readonly Dictionary<string, Action<T, object>> setter = new Dictionary<string, Action<T, object>>();
        public PropertyContainer()
        {
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                if (Attribute.IsDefined(propertyInfo, typeof(InversePropertyAttribute)) != true)
                {
                    var property = new Property(propertyInfo);
                    getter.Add(property.Name, propertyInfo.GetValueGetter<T>());
                    setter.Add(property.Name, propertyInfo.GetValueSetter<T>());
                    if (property.IsKey) GetProperties.Insert(0, property); else GetProperties.Add(property);
                }
            }
            properties = GetProperties.ToDictionary(p => p.Name);
        }
        public bool Contains(string property) => properties.ContainsKey(property);
        public Property Get(string property) => Contains(property) ? properties[property] : null;
        public Func<T, object> Getter(string property) => Contains(property) ? getter[property] : null;
        public Action<T, object> Setter(string property) => Contains(property) ? setter[property] : null;
        public List<Property> GetProperties { get; } = new List<Property>();
    }
}
