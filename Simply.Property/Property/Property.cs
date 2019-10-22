using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Simply.Property
{
    internal class Property<T>
    {
        public Property(PropertyInfo property)
        {
            var columnAttribute = property.GetAttribute<ColumnAttribute>();
            var xmlAttribute = property.GetAttribute<XmlPropertyAttribute>();
            var jsonAttribute = property.GetAttribute<JsonPropertyAttribute>();
            var generatedAttribute = property.GetAttribute<DatabaseGeneratedAttribute>();
            var maxLengthAttribute = property.GetAttribute<MaxLengthAttribute>();
            Name = property.Name;
            Type = property.PropertyType;
            DeclaringType = property.DeclaringType;
            IsKey = Attribute.IsDefined(property, typeof(KeyAttribute));
            IsRequired = Attribute.IsDefined(property, typeof(RequiredAttribute)) || !(Nullable.GetUnderlyingType(property.PropertyType) != null);
            IsIdentity = (generatedAttribute != null) ? generatedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity : false;
            JsonIgnore = Attribute.IsDefined(property, typeof(JsonIgnoreAttribute));
            JsonProperty = (jsonAttribute == null ? property.Name : jsonAttribute.PropertyName);
            XmlProperty = (xmlAttribute == null ? property.Name : xmlAttribute.PropertyName);
            MaxLength = maxLengthAttribute?.Length;
            ColumnName = (columnAttribute == null ? property.Name : columnAttribute.Name);
            PropertyInfo = property;
        }
        public bool IsKey { get; set; }
        public bool IsRequired { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsNonClusteredIndex { get; set; }
        public bool JsonIgnore { get; set; }
        public string Name { get; set; }
        public string ColumnName { get; set; }
        public string JsonProperty { get; set; }
        public string XmlProperty { get; set; }
        public int? MaxLength { get; set; }
        public Type Type { get; set; }
        public Type DeclaringType { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
    }
}
