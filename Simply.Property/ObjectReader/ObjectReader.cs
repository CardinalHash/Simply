using System;
using System.Collections.Generic;
using System.Xml;
using System.Threading.Tasks;
using System.Linq;

namespace Simply.Property
{
    public class ObjectReader : IDisposable
    {
        private const int defaultTaskCount = 5;
        private const int defaultBlockSize = 1000;
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
        public ObjectReader(IPropertyScope scope)
        {
            container = new ObjectContainer(defaultTaskCount, defaultBlockSize);
            this.scope = scope;
        }
        public ObjectReader handle<T>(Func<IEnumerable<T>, Task> blockActionAsync)
        {
            container.handle(blockActionAsync, scope.property<T>().ToDictionary(p => p.xmlProperty));
            return this;
        }
        public int getObject(string uri)
        {
            using (XmlReader xmlReader = XmlReader.Create(uri))
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
            }
            return overall;
        }
        public int overall => container.overall;
        public void Dispose()
        {
            container.Dispose();
        }
    }
}
