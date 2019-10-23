using System;

namespace Simply.Property
{
    /// <summary>
    /// Описание привязки свойств класса к тегам xml-документа
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class XmlPropertyAttribute : Attribute
    {
        /// <summary>
        /// Свойство для идентификации, будет содержать порядок элемента в файле xml
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// Описание привязки свойств класса к тегам xml-документа
        /// </summary>
        /// <param name="propertyName">Свойство для идентификации, будет содержать порядок элемента в файле xml</param>
        public XmlPropertyAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }
    }
}
