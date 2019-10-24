using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Simply.Property
{
    /// <summary>
    /// Описание свойства класса
    /// </summary>
    public class Property
    {
        /// <summary>
        /// Создать описание свойства
        /// </summary>
        /// <param name="property"></param>
        public Property(PropertyInfo property)
        {
            PropertyInfo = property;
            IsKey = Attribute.IsDefined(property, typeof(KeyAttribute));
            IsRequired = Attribute.IsDefined(property, typeof(RequiredAttribute)) || Type.IsValueType && Nullable.GetUnderlyingType(Type) == null;
            var generatedAttribute = property.GetAttribute<DatabaseGeneratedAttribute>();
            IsIdentity = (generatedAttribute != null) ? generatedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity : false;
            NotMapped = Attribute.IsDefined(property, typeof(NotMappedAttribute));
            JsonIgnore = Attribute.IsDefined(property, typeof(JsonIgnoreAttribute));
            var columnAttribute = property.GetAttribute<ColumnAttribute>();
            ColumnName = (columnAttribute == null ? property.Name : columnAttribute.Name);
            var xmlAttribute = property.GetAttribute<XmlPropertyAttribute>();
            XmlProperty = (xmlAttribute == null ? property.Name : xmlAttribute.PropertyName);
            var jsonAttribute = property.GetAttribute<JsonPropertyAttribute>();
            JsonProperty = (jsonAttribute == null ? property.Name : jsonAttribute.PropertyName);
            var maxLengthAttribute = property.GetAttribute<MaxLengthAttribute>();
            MaxLength = maxLengthAttribute?.Length;
        }
        /// <summary>
        /// Это ключевое поле?
        /// </summary>
        public bool IsKey { get; set; }
        /// <summary>
        /// Это обязательное для заполнения поле?
        /// </summary>
        public bool IsRequired { get; set; }
        /// <summary>
        /// Это поле типа Identity?
        /// </summary>
        public bool IsIdentity { get; set; }
        /// <summary>
        /// Игнорировать ли поле при сериализации или десериализации в формат json?
        /// </summary>
        public bool JsonIgnore { get; set; }
        /// <summary>
        /// Игнорировать ли поле при проецировании в базу данных?
        /// </summary>
        public bool NotMapped { get; set; }
        /// <summary>
        /// Наименование поля
        /// </summary>
        public string Name => PropertyInfo.Name;
        /// <summary>
        /// Наименование поля в базе данных
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// Наименование поля в xml-файле
        /// </summary>
        public string XmlProperty { get; set; }
        /// <summary>
        /// Наименование поля в json-файле
        /// </summary>
        public string JsonProperty { get; set; }
        /// <summary>
        /// Максимальная длинна поле, если применимо
        /// </summary>
        public int? MaxLength { get; set; }
        /// <summary>
        /// Тип поля значимый или нет?
        /// </summary>
        public bool IsValueType => Type.IsValueType;
        /// <summary>
        /// Тип поля
        /// </summary>
        public Type Type => PropertyInfo.PropertyType;
        /// <summary>
        /// Базовый класс в котором объявлено поле
        /// </summary>
        public Type DeclaringType => PropertyInfo.DeclaringType;
        /// <summary>
        /// Свойства поля
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }
    }
}
