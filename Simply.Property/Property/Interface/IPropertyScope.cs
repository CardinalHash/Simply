namespace Simply.Property
{
    internal interface IPropertyScope
    {
        IPropertyManager<T> Property<T>();
    }
}
