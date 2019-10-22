using System;

namespace Simply.Property
{
    internal interface IObjectEntity
    {
        int Count { get; }
        string Upper { get; }
        string Name { get; }
        Guid ObjId { get; }
        bool Property(string name);
        void Add(string property, string value);
        void Push();
        void Pop(IObjectEntity upper = null);
        void Clear();
    }
}
