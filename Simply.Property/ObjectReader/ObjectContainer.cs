using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Simply.Property
{
    internal class ObjectContainer : IDisposable
    {
        private readonly JsonSerializerSettings defaultJsonSettings = new JsonSerializerSettings { Error = (sender, args) => args.ErrorContext.Handled = true };
        private readonly SemaphoreSlim semaphore;
        private readonly int defaultBlockSize;
        private readonly int defaultTaskCount;
        private readonly Stack<string> stack;
        private readonly IList<string> typeList;
        private readonly IDictionary<string, IObjectEntity> types;
        public ObjectContainer(int defaultTaskCount, int defaultBlockSize)
        {
            this.defaultTaskCount = defaultTaskCount;
            this.defaultBlockSize = defaultBlockSize;
            this.semaphore = new SemaphoreSlim(defaultTaskCount);
            this.types = new Dictionary<string, IObjectEntity>();
            this.typeList = new List<string>();
            this.stack = new Stack<string>();
        }
        private void addHandle<T>(ObjectEntity<T> obj)
        {
            typeList.Add(obj.Name);
            types.Add(obj.Name, obj);
        }
        public void Handle<T>(Func<IEnumerable<T>, Task> blockActionAsync, Dictionary<string, Property<T>> properties)
        {
            addHandle(new ObjectEntity<T>(properties, defaultBlockSize, async(json) =>
            {
                semaphore.Wait();
                try
                {
                    await blockActionAsync(JsonConvert.DeserializeObject<IEnumerable<T>>(json, defaultJsonSettings)).ConfigureAwait(false);
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }
        public void Handle<T>(Func<string, Task> blockActionAsync, Dictionary<string, Property<T>> properties)
        {
            addHandle(new ObjectEntity<T>(properties, defaultBlockSize, async (json) =>
            {
                semaphore.Wait();
                try
                {
                    await blockActionAsync(json).ConfigureAwait(false);
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }
        public void Add(string name, string property, string value)
        {
            if (types.ContainsKey(name))
                types[name].Add(property, value);
        }
        public void Push(string name)
        {
            stack.Push(name);
            if (types.ContainsKey(name))
                types[name].Push();
        }
        public void Pop()
        {
            string name = stack.Pop();
            if (types.ContainsKey(name))
            {
                var upper = types[name].Upper;
                if (upper != null && types.ContainsKey(upper))
                    types[name].Pop(types[upper]);
                else
                    types[name].Pop();
            }
        }
        public bool Contains(string name) => types.ContainsKey(name);
        public bool Property(string name, string property) => types[name].Property(property);
        public string Peek() => stack.Peek();
        public int Count(string name) => types[name].Count;
        public int Count() => stack.Count;
        public void Clear()
        {
            // сохраняем остаточные данные
            foreach (var entry in types.Values)
                entry.Clear();
            // занимаем ресурсы
            for (int task = 0; task < defaultTaskCount; task++)
                semaphore.Wait();
            // освобождаем ресурсы
            for (int task = 0; task < defaultTaskCount; task++)
                semaphore.Release();
        }
        public int Overall => typeList.Select(entry => Count(entry)).Sum();
        public void Dispose()
        {
            semaphore.Dispose();
        }
    }
}
