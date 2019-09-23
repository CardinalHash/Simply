namespace Simply.Property
{
    public interface IPropertyScope
    {
        IPropertyManager<T> property<T>();
        void setter<T>(string property, T item, object value);
        object getter<T>(string property, T item);
    }
}
