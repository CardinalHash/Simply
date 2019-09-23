using System;
using System.Reflection;

namespace Simply.Property
{
    public class Property<T>
    {
        public bool isKey { get; set; }
        public bool isIdentity { get; set; }
        public bool isNonClusteredIndex { get; set; }
        public bool jsonIgnore { get; set; }
        public string name { get; set; }
        public string jsonProperty { get; set; }
        public string xmlProperty { get; set; }
        public int? maxLength { get; set; }
        public string column { get; set; }
        public Type type { get; set; }
        public Type declaringType { get; set; }
        public PropertyInfo property { get; set; }
        public Func<T, object> getter { get; set; }
        public Action<T, object> setter { get; set; }
    }
}
