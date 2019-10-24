using System.Linq;
using System.Collections.Generic;
using System;

namespace Simply.Property
{
    /// <summary>
    /// Класс для работы с полями класса
    /// </summary>
    public class PropertyScope : IPropertyScope
    {
        private SynchronizedCache<string, object> propertyScope = new SynchronizedCache<string, object>();
        private PropertyContainer<T> getContainer<T>() => (PropertyContainer<T>)propertyScope.GetOrCreate(typeof(PropertyContainer<T>).ToString(), () => new PropertyContainer<T>());
        /// <summary>
        /// Получить список полей класса
        /// </summary>
        /// <typeparam name="T">Тип класса для анализа</typeparam>
        /// <returns></returns>
        public IList<Property> GetProperties<T>() => getContainer<T>().GetProperties;
        /// <summary>
        /// Задать значение свойства класса
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="item">Объект типа T</param>
        /// <param name="property">Свойство класса для установки значения</param>
        /// <param name="value">Объект, который будет назначен свойству</param>
        public void Setter<T>(T item, string property, object value) => getContainer<T>()?.Setter(property)?.Invoke(item, value);
        /// <summary>
        /// Получить значение свойства класса
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="item">Объект типа T</param>
        /// <param name="property">Свойство для считывания значения</param>
        /// <returns></returns>
        public object Getter<T>(T item, string property) => getContainer<T>()?.Getter(property)(item);
        /// <summary>
        /// Содержит ли класс указанное свойство?
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="property">Наименования свойства для поиска</param>
        /// <returns></returns>
        public bool Contains<T>(string property) => getContainer<T>().Contains(property);
        /// <summary>
        /// Получить свойство указанного класса
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="property">Наименования свойства</param>
        /// <returns></returns>
        public Property Get<T>(string property) => Contains<T>(property) ? getContainer<T>().Get(property) : null;
    }
}
