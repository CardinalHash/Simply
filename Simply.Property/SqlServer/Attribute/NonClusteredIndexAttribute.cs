using System;

namespace Simply.Property.SqlServer
{
    public class NonClusteredIndexAttribute : Attribute
    {
        public NonClusteredIndexAttribute(string name, string[] properties)
        {
            Properties = properties;
            Name = name;
        }
        public string[] Properties { get; set; }
        public string Name { get; set; }
    }
}
