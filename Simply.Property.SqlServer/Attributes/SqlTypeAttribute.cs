using System;

namespace Simply.Property.SqlServer
{
    /// <summary>
    /// Атрибут для связывания поля класса и типа столбца в базе данных
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SqlTypeAttribute : Attribute
    {
        /// <summary>
        /// Тип столбца таблицы в базе данных
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Тип столбца таблицы в базе данных
        /// </summary>
        /// <param name="type">Тип столбца таблицы в базе данных</param>
        public SqlTypeAttribute(string type)
        {
            this.Type = type;
        }
    }
}
