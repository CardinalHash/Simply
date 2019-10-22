using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Collections;

namespace Simply.Property
{
    internal class PropertyManager<T> : IPropertyManager<T>
    {
        private Dictionary<string, Property<T>> properties;
        private List<Property<T>> propertyList;
        public PropertyManager()
        {
            getProperties();
        }
        private void getProperties()
        {
            propertyList = new List<Property<T>>();
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                if (propertyInfo.GetAttribute<InversePropertyAttribute>() == null)
                {
                    var property = new Property<T>(propertyInfo);
                    if (property.IsKey) propertyList.Insert(0, property); else propertyList.Add(property);
                }
            }
            properties = propertyList.ToDictionary(p => p.Name);
        }
        public bool Contains(string property) => properties.ContainsKey(property);
        public Property<T> Get(string property) => Contains(property) ? properties[property] : null;
        public IEnumerator<Property<T>> GetEnumerator() => propertyList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => propertyList.GetEnumerator();
    }
}
