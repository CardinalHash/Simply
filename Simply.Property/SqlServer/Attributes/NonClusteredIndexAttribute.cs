using System;

namespace Simply.Property.SqlServer
{
    /// <summary>
    /// Создание индекса на указанных полях таблицы
    /// </summary>
    public class NonClusteredIndexAttribute : Attribute
    {
        /// <summary>
        /// Создание индекса на указанных полях таблицы
        /// </summary>
        /// <param name="name">Название индекса</param>
        /// <param name="properties">Поля таблицы для создания индекса</param>
        public NonClusteredIndexAttribute(string name, string[] properties)
        {
            Properties = properties;
            Name = name;
        }
        /// <summary>
        /// Поля таблицы для создания индекса
        /// </summary>
        public string[] Properties { get; set; }
        /// <summary>
        /// Название индекса
        /// </summary>
        public string Name { get; set; }
    }
}
