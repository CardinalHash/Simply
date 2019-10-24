using System.Collections.Generic;

namespace Simply.Property
{
    /// <summary>
    /// Класс для работы с полями класса
    /// </summary>
    public interface IPropertyScope
    {
        /// <summary>
        /// Получить список полей класса
        /// </summary>
        /// <typeparam name="T">Тип класса для анализа</typeparam>
        /// <returns></returns>
        IList<Property> GetProperties<T>();
        /// <summary>
        /// Задать значение свойства класса
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="item">Объект типа T</param>
        /// <param name="property">Свойство класса для установки значения</param>
        /// <param name="value">Объект, который будет назначен свойству</param>
        void Setter<T>(T item, string property, object value);
        /// <summary>
        /// Получить значение свойства класса
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="item">Объект типа T</param>
        /// <param name="property">Свойство для считывания значения</param>
        /// <returns></returns>
        object Getter<T>(T item, string property);
        /// <summary>
        /// Содержит ли класс указанное свойство?
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="property">Наименования свойства для поиска</param>
        /// <returns></returns>
        bool Contains<T>(string property);
        /// <summary>
        /// Получить свойство указанного класса
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="property">Наименования свойства</param>
        /// <returns></returns>
        Property Get<T>(string property);
    }
}
