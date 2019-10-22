using System;

namespace Simply.Property
{
    [AttributeUsage(AttributeTargets.Property)]
    public class XmlPropertyAttribute : Attribute
    {
        public string PropertyName { get; set; }
        /// <summary>
        /// Описание привязки класса к тегам xml-документа
        /// </summary>
        /// <param name="propertyName">Свойство для идентификации, будет содержать порядок элемента в файле xml</param>
        public XmlPropertyAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }
    }
}
