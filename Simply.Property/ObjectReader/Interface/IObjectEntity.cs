using System;

namespace Simply.Property
{
    public interface IObjectEntity
    {
        int count { get; }
        string upper { get; }
        string name { get; }
        Guid objId { get; }
        bool property(string name);
        void add(string property, string value);
        void push();
        void pop(IObjectEntity upper = null);
        void clear();
    }
}
