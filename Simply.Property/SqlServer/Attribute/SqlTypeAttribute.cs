using System;

namespace Simply.Property.SqlServer
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SqlTypeAttribute : Attribute
    {
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
