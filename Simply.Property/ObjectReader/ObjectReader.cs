using System;
using System.Collections.Generic;
using System.Xml;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text;

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
            var name = container.peek();
            for (int attribute = 0; attribute < xmlReader.AttributeCount; attribute++)
            {
                xmlReader.MoveToAttribute(attribute);
                if (container.property(name, xmlReader.Name))
                    container.add(name, xmlReader.Name, xmlReader.Value);
            }
            xmlReader.MoveToElement();
        }
        /// <summary>
        /// Создать класс чтения xml-файлов
        /// </summary>
        /// <param name="defaultBlockSize">количество объектов в блоке</param>
        /// <param name="defaultTaskCount">количество одновременно обрабатываемых блоков</param>
        public ObjectReader(int defaultBlockSize = 1000, int defaultTaskCount = 5)
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
        public ObjectReader handle<T>(Func<IEnumerable<T>, Task> blockActionAsync)
        {
            container.handle(blockActionAsync, scope.property<T>().ToDictionary(p => p.xmlProperty));
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
        public ObjectReader handle<T>(Func<string, Task> blockActionAsync)
        {
            container.handle(blockActionAsync, scope.property<T>().ToDictionary(p => p.xmlProperty));
            return this;
        }
        private int getObject(XmlReader xmlReader)
        {
            while (xmlReader.Read())
            {
                var xmlName = xmlReader.Name.Replace("-", "");
                if (container.count() != 0 && container.property(container.peek(), xmlName) && xmlReader.NodeType == XmlNodeType.Element)
                {
                    xmlReader.Read();
                    container.add(container.peek(), xmlName, xmlReader.Value);
                }
                if (container.contains(xmlName) && xmlReader.NodeType == XmlNodeType.Element)
                {
                    container.push(xmlName);
                    get(xmlReader);
                    if (xmlReader.IsEmptyElement)
                    {
                        container.pop();
                    }
                }
                if (container.count() != 0 && container.peek() == xmlName && container.contains(xmlName) && xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    container.pop();
                }
            }
            container.clear();
            return overall;
        }
        /// <summary>
        /// Обработка xml-файла
        /// </summary>
        /// <param name="uri">Путь к xml-файлу</param>
        /// <returns></returns>
        public int getObject(string uri)
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
        public int getObject(MemoryStream xml)
        {
            using (XmlReader xmlReader = XmlReader.Create(xml))
            {
                return getObject(xmlReader);
            }
        }
        /// <summary>
        /// Общее количество обработанных объектов
        /// </summary>
        public int overall => container.overall;
        /// <summary>
        /// Освобождение ресузсов загрузчика
        /// </summary>
        public void Dispose()
        {
            container.Dispose();
        }
    }
}
