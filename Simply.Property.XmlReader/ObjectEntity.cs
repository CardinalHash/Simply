using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Simply.Property
{
    internal class ObjectEntity<T> : IObjectEntity
    {
        private readonly XmlSchemaAttribute schema;
        private readonly StringBuilder container, obj, temp;
        private readonly Func<string, Task> blockActionAsync;
        private readonly Dictionary<string, Property> properties;
        private readonly int defaultBlockSize;
        public ObjectEntity(Dictionary<string, Property> properties, int defaultBlockSize, Func<string, Task> blockActionAsync)
        {
            Count = 0;
            ObjId = Guid.NewGuid();
            obj = new StringBuilder();
            temp = new StringBuilder();
            container = new StringBuilder("[");
            this.defaultBlockSize = defaultBlockSize;
            this.blockActionAsync = blockActionAsync;
            this.properties = properties;
            schema = Attribute.GetCustomAttribute(typeof(T), typeof(XmlSchemaAttribute)) as XmlSchemaAttribute;
        }
        public bool Property(string name) => properties.ContainsKey(name);
        public Guid ObjId { get; private set; }
        public int Count { get; private set; }
        public string Name => schema.Name;
        public string PropertyName => schema.PropertyName;
        public string Upper => schema.Upper;
        public string UpperPropertyName => schema.UpperPropertyName;
        public void Add(string property, string value) => obj.Append($"\"{properties[property].JsonProperty}\":{(value.Length != 0 ? $"\"{value.ToValue(temp)}\"" : "null")},");
        public void Push()
        {
            obj.Clear();
            ObjId = Guid.NewGuid();
            Count++;
        }
        public void Pop(IObjectEntity upper = null)
        {
            container.Append("{").Append(obj);
            if (PropertyName != null)
                container.Append($"\"{PropertyName}\":\"{ObjId}\",");
            if (upper != null && UpperPropertyName != null)
                container.Append($"\"{UpperPropertyName}\":\"{upper.ObjId}\",");
            container.Remove(container.Length - 1, 1).Append("},");
            if (Count % defaultBlockSize == 0)
                Clear();
        }
        public void Clear()
        {
            if (container.Length != 1)
            {
                container.Remove(container.Length - 1, 1);
                blockActionAsync(container.Append("]").ToString());
                container.Clear().Append("[");
            }
        }
    }
}
