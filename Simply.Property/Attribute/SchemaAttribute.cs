using System;

namespace Simply.Property
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SchemaAttribute : Attribute
    {
        public string Name { get; }
        public string PropertyName { get; set; }
        public string Upper { get; }
        public string UpperPropertyName { get; }
        /// <summary>
        /// Описание привязки класса к тегам xml-документа
        /// </summary>
        /// <param name="name">Тег для идентификации класса</param>
        /// <param name="propertyName">Свойство для идентификации, будет содержать уникальный идентификатор класса</param>
        /// <param name="upper">Тег верхнего уровня</param>
        /// <param name="upperPropertyName">Cвойство для идентификации класса верхнего уровня</param>
        public SchemaAttribute(string name, string propertyName, string upper, string upperPropertyName)
        {
            this.Name = name;
            this.PropertyName = propertyName;
            this.Upper = upper;
            this.UpperPropertyName = upperPropertyName;
        }
        public SchemaAttribute(string name, string propertyName)
            : this(name, propertyName, null, null)
        {
        }
        public SchemaAttribute(string name)
            : this(name, null, null, null)
        {
        }
    }
}
