using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Intellegens.Commons.Types
{
    public static class TypeUtils
    {
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> typePropertiesCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();

        public static List<PropertyInfo> GetProperties<T>()
            where T: class
        {
            return typePropertiesCache.GetOrAdd(typeof(T), (Type type) => type.GetProperties().ToList());
        }

        public static PropertyInfo GetProperty<T>(string propertyName, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where T : class
        {
            return GetProperties<T>()
                .Where(x => x.Name.Equals(propertyName, comparison))
                .First();
        }
    }
}
