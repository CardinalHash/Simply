using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Collections;
using Newtonsoft.Json;

namespace Simply.Property
{
    public class PropertyManager<T> : IPropertyManager<T>
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
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                var columnAttribute = property.GetAttribute<ColumnAttribute>();
                var xmlAttribute = property.GetAttribute<XmlPropertyAttribute>();
                var jsonAttribute = property.GetAttribute<JsonPropertyAttribute>();
                var generatedAttribute = property.GetAttribute<DatabaseGeneratedAttribute>();
                var maxLengthAttribute = property.GetAttribute<MaxLengthAttribute>();
                var prop = new Property<T>
                {
                    name = property.Name,
                    type = property.PropertyType,
                    declaringType = property.DeclaringType,
                    isKey = Attribute.IsDefined(property, typeof(KeyAttribute)),
                    isRequired = Attribute.IsDefined(property, typeof(RequiredAttribute)) || !(Nullable.GetUnderlyingType(property.PropertyType) != null),
                    isIdentity = (generatedAttribute != null) ? generatedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity : false,
                    jsonIgnore = Attribute.IsDefined(property, typeof(JsonIgnoreAttribute)),
                    jsonProperty = (jsonAttribute == null ? property.Name : jsonAttribute.PropertyName),
                    xmlProperty = (xmlAttribute == null ? property.Name : xmlAttribute.PropertyName),
                    maxLength = maxLengthAttribute?.Length,
                    column = (columnAttribute == null ? property.Name : columnAttribute.Name),
                    property = property
                };
                if (property.GetAttribute<InversePropertyAttribute>() == null)
                {
                    if (prop.isKey) propertyList.Insert(0, prop); else propertyList.Add(prop);
                }
            }
            properties = propertyList.ToDictionary(p => p.name);
        }
        public bool contains(string property) => properties.ContainsKey(property);
        public Property<T> get(string property) => contains(property) ? properties[property] : null;
        public IEnumerator<Property<T>> GetEnumerator() => propertyList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => propertyList.GetEnumerator();
    }
}
