using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Simply.Property
{
    public class ObjectContainer : IDisposable
    {
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
        public void handle<T>(Func<IEnumerable<T>, Task> blockActionAsync, Dictionary<string, Property<T>> properties)
        {
            var obj = new ObjectEntity<T>(properties, defaultBlockSize, async(entities) =>
            {
                semaphore.Wait();
                await blockActionAsync(entities).ConfigureAwait(false);
                semaphore.Release();
            });
            typeList.Add(obj.name);
            types.Add(obj.name, obj);
        }
        public void add(string name, string property, string value)
        {
            if (types.ContainsKey(name))
                types[name].add(property, value);
        }
        public void push(string name)
        {
            stack.Push(name);
            if (types.ContainsKey(name))
                types[name].push();
        }
        public void pop()
        {
            string name = stack.Pop();
            if (types.ContainsKey(name))
            {
                var upper = types[name].upper;
                if (upper != null && types.ContainsKey(upper))
                    types[name].pop(types[upper]);
                else
                    types[name].pop();
            }
        }
        public bool contains(string name) => types.ContainsKey(name);
        public bool property(string name, string property) => types[name].property(property);
        public string peek() => stack.Peek();
        public int count(string name) => types[name].count;
        public int count() => stack.Count;
        public void clear()
        {
            // сохраняем остаточные данные
            foreach (var entry in types.Values)
                entry.clear();
            // занимаем ресурсы
            for (int task = 0; task < defaultTaskCount; task++)
                semaphore.Wait();
            // освобождаем ресурсы
            for (int task = 0; task < defaultTaskCount; task++)
                semaphore.Release();
        }
        public int overall => typeList.Select(entry => count(entry)).Sum();
        public void Dispose()
        {
            semaphore.Dispose();
        }
    }
}
