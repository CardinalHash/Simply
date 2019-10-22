namespace Simply.Property
{
    internal class PropertyScope : IPropertyScope
    {
        private SynchronizedCache<string, object> propertyScope = new SynchronizedCache<string, object>();
        public IPropertyManager<T> Property<T>() => (IPropertyManager<T>)propertyScope.GetOrCreate(typeof(IPropertyManager<T>).ToString(), () => new PropertyManager<T>());
    }
}
