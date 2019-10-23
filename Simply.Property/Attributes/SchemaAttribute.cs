using System;

namespace Simply.Property
{
    /// <summary>
    /// Описание привязки класса данных к тегам xml-документа
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SchemaAttribute : Attribute
    {
        /// <summary>
        /// Тег для идентификации класса в xml-документе
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Свойство для идентификации, будет содержать уникальный идентификатор класса
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// Тег класса данных верхнего уровня
        /// </summary>
        public string Upper { get; }
        /// <summary>
        /// Cвойство для идентификации класса верхнего уровня
        /// </summary>
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
        /// <summary>
        /// Описание привязки класса к тегам xml-документа
        /// </summary>
        /// <param name="name">Тег для идентификации класса</param>
        /// <param name="propertyName">Свойство для идентификации, будет содержать уникальный идентификатор класса</param>
        public SchemaAttribute(string name, string propertyName)
            : this(name, propertyName, null, null)
        {
        }
        /// <summary>
        /// Описание привязки класса к тегам xml-документа
        /// </summary>
        /// <param name="name">Тег для идентификации класса</param>
        public SchemaAttribute(string name)
            : this(name, null, null, null)
        {
        }
    }
}
