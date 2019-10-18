using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Simply.Property
{
    internal class ObjectEntity<T> : IObjectEntity
    {
        private readonly SchemaAttribute schema;
        private readonly StringBuilder container, obj, temp;
        private readonly Func<string, Task> blockActionAsync;
        private readonly Dictionary<string, Property<T>> properties;
        private readonly int defaultBlockSize;
        public ObjectEntity(Dictionary<string, Property<T>> properties, int defaultBlockSize, Func<string, Task> blockActionAsync)
        {
            count = 0;
            objId = Guid.NewGuid();
            obj = new StringBuilder();
            temp = new StringBuilder();
            container = new StringBuilder("[");
            this.defaultBlockSize = defaultBlockSize;
            this.blockActionAsync = blockActionAsync;
            this.properties = properties;
            schema = Attribute.GetCustomAttribute(typeof(T), typeof(SchemaAttribute)) as SchemaAttribute;
        }
        public bool property(string name) => properties.ContainsKey(name);
        public Guid objId { get; private set; }
        public int count { get; private set; }
        public string name => schema.Name;
        public string propertyName => schema.PropertyName;
        public string upper => schema.Upper;
        public string upperPropertyName => schema.UpperPropertyName;
        public void add(string property, string value) => obj.Append($"\"{properties[property].jsonProperty}\":{(value.Length != 0 ? $"\"{value.ToXmlValue(temp)}\"" : "null")},");
        public void push()
        {
            obj.Clear();
            objId = Guid.NewGuid();
            count++;
        }
        public void pop(IObjectEntity upper = null)
        {
            container.Append("{").Append(obj);
            if (propertyName != null)
                container.Append($"\"{propertyName}\":\"{objId}\",");
            if (upper != null && upperPropertyName != null)
                container.Append($"\"{upperPropertyName}\":\"{upper.objId}\",");
            container.Remove(container.Length - 1, 1).Append("},");
            if (count % defaultBlockSize == 0)
                clear();
        }
        public void clear()
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
