using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simply.Property
{
    /// <summary>
    /// Класс для загрузки xml-файлов
    /// </summary>
    public class ObjectReader : IDisposable
    {
        private readonly ObjectContainer container;
        private readonly IPropertyScope scope;
        private void get(XmlReader xmlReader)
        {
            var name = container.Peek();
            for (int attribute = 0; attribute < xmlReader.AttributeCount; attribute++)
            {
                xmlReader.MoveToAttribute(attribute);
                if (container.Property(name, xmlReader.Name))
                    container.Add(name, xmlReader.Name, xmlReader.Value);
            }
            xmlReader.MoveToElement();
        }
        /// <summary>
        /// Создать класс чтения xml-файлов
        /// </summary>
        /// <param name="defaultBlockSize">количество объектов в блоке</param>
        /// <param name="defaultTaskCount">количество одновременно обрабатываемых блоков</param>
        public ObjectReader(int defaultBlockSize = 10000, int defaultTaskCount = 3)
        {
            container = new ObjectContainer(defaultTaskCount, defaultBlockSize);
            scope = new PropertyScope();
        }
        /// <summary>
        /// Задаем обработчик для загруженных данных из xml-файла
        /// Обработчик вызывается по мере накопления данных, обработка происходит блоками
        /// Данные возвращаются в виде коллекции объектов типа T
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="blockActionAsync">Делегат, который вызывается после накопления необходимого числа объектов типа T</param>
        /// <returns></returns>
        public ObjectReader HandleObjects<T>(Func<IEnumerable<T>, Task> blockActionAsync)
        {
            container.Handle<T>(blockActionAsync, scope.GetProperties<T>().ToDictionary(p => p.XmlProperty));
            return this;
        }
        /// <summary>
        /// Задаем обработчик для загруженных данных из xml-файла
        /// Обработчик вызывается по мере накопления данных, обработка происходит блоками
        /// Данные возвращаются в ввиде json-строки
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="blockActionAsync">Делегат, который вызывается после накопления необходимого числа объектов типа T</param>
        /// <returns></returns>
        public ObjectReader HandleString<T>(Func<string, Task> blockActionAsync)
        {
            container.Handle<T>(blockActionAsync, scope.GetProperties<T>().ToDictionary(p => p.XmlProperty));
            return this;
        }
        private int getObject(XmlReader xmlReader)
        {
            while (xmlReader.Read())
            {
                var xmlName = xmlReader.Name.Replace("-", "");
                if (container.Count() != 0 && container.Property(container.Peek(), xmlName) && xmlReader.NodeType == XmlNodeType.Element)
                {
                    xmlReader.Read();
                    container.Add(container.Peek(), xmlName, xmlReader.Value);
                }
                if (container.Contains(xmlName) && xmlReader.NodeType == XmlNodeType.Element)
                {
                    container.Push(xmlName);
                    get(xmlReader);
                    if (xmlReader.IsEmptyElement)
                    {
                        container.Pop();
                    }
                }
                if (container.Count() != 0 && container.Peek() == xmlName && container.Contains(xmlName) && xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    container.Pop();
                }
            }
            container.Clear();
            return Overall;
        }
        /// <summary>
        /// Обработка xml-файла
        /// </summary>
        /// <param name="uri">Путь к xml-файлу</param>
        /// <returns></returns>
        public int GetObject(string uri)
        {
            using (XmlReader xmlReader = XmlReader.Create(uri))
            {
                return getObject(xmlReader);
            }
        }
        /// <summary>
        /// Обработка xml-документа
        /// </summary>
        /// <param name="xml">xml-документ</param>
        /// <returns></returns>
        public int GetObject(MemoryStream xml)
        {
            using (XmlReader xmlReader = XmlReader.Create(xml))
            {
                return getObject(xmlReader);
            }
        }
        /// <summary>
        /// Общее количество обработанных объектов
        /// </summary>
        public int Overall => container.Overall;
        /// <summary>
        /// Освобождение ресузсов загрузчика
        /// </summary>
        public void Dispose()
        {
            container.Dispose();
        }
    }
}
