using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Simply.Property
{
    internal class JsonPropertySerializerContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, HashSet<string>> properties;
        private readonly Dictionary<Type, HashSet<string>> ignoreProperties;
        private readonly Dictionary<Type, Dictionary<string, string>> renameProperties;
        public JsonPropertySerializerContractResolver()
        {
            properties = new Dictionary<Type, HashSet<string>>();
            ignoreProperties = new Dictionary<Type, HashSet<string>>();
            renameProperties = new Dictionary<Type, Dictionary<string, string>>();
        }
        public JsonPropertySerializerContractResolver Property(Type t, params string[] jsonProperties)
        {
            if (properties.ContainsKey(t) != true)
                properties[t] = new HashSet<string>();
            jsonProperties.ForEach(property => properties[t].Add(property));
            return this;
        }
        public JsonPropertySerializerContractResolver Property<T>(params Property<T>[] jsonProperties)
        {
            jsonProperties.GroupBy(p => p.DeclaringType).ForEach(property => Property(property.Key, property.Select(p => p.JsonProperty).ToArray()));
            return this;
        }
        public JsonPropertySerializerContractResolver IgnoreProperty(Type t, params string[] jsonProperties)
        {
            if (ignoreProperties.ContainsKey(t) != true) ignoreProperties[t] = new HashSet<string>();
            jsonProperties.ForEach(property => ignoreProperties[t].Add(property));
            return this;
        }
        public JsonPropertySerializerContractResolver IgnoreProperty<T>(params Property<T>[] jsonProperties)
        {
            jsonProperties.GroupBy(p => p.DeclaringType).ForEach(property => IgnoreProperty(property.Key, property.Select(p => p.JsonProperty).ToArray()));
            return this;
        }
        public JsonPropertySerializerContractResolver RenameProperty(Type type, string propertyName, string newJsonPropertyName)
        {
            if (renameProperties.ContainsKey(type) != true)
            {
                renameProperties[type] = new Dictionary<string, string>();
            }
            renameProperties[type][propertyName] = newJsonPropertyName;
            return this;
        }
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (isPresent(property.DeclaringType, property.PropertyName) != true)
            {
                property.ShouldSerialize = i => false;
            }
            if (isIgnored(property.DeclaringType, property.PropertyName))
            {
                property.ShouldSerialize = i => false;
            }
            if (isRenamed(property.DeclaringType, property.PropertyName, out var newJsonPropertyName))
            {
                property.PropertyName = newJsonPropertyName;
            }
            return property;
        }
        private bool isPresent(Type type, string jsonPropertyName) => properties.ContainsKey(type) != true || properties.ContainsKey(type) && properties[type].Contains(jsonPropertyName);
        private bool isIgnored(Type type, string jsonPropertyName) => ignoreProperties.ContainsKey(type) && ignoreProperties[type].Contains(jsonPropertyName);
        private bool isRenamed(Type type, string jsonPropertyName, out string newJsonPropertyName)
        {
            if (!renameProperties.TryGetValue(type, out Dictionary<string, string> renames) || !renames.TryGetValue(jsonPropertyName, out newJsonPropertyName))
            {
                newJsonPropertyName = null;
                return false;
            }
            return true;
        }
    }
}
