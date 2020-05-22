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

        /// <summary>
        /// Returns all properties for type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<PropertyInfo> GetProperties<T>()
            where T: class
        {
            return GetProperties(typeof(T));
        }

        /// <summary>
        /// Returns all properties for type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<PropertyInfo> GetProperties(Type type)
        {
            return typePropertiesCache.GetOrAdd(type, (Type type) => type.GetProperties().ToList());
        }

        /// <summary>
        /// Returns property info for given type. Function can accept nested property name, eg. "Parent.ParentId"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Property name ("Id"), or nested property expression ("Parent.Id")</param>
        /// <param name="comparison">String comparison type</param>
        /// <returns></returns>
        public static PropertyInfo GetProperty<T>(string propertyName, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where T : class
        {
            List<string> chain = propertyName.Split('.').ToList();
            PropertyInfo propertyInfo = null;
            
            foreach(var prop in chain)
            {
                var propertyType = propertyInfo?.DeclaringType ?? typeof(T);

                propertyInfo = GetProperties(propertyType)
                .Where(x => x.Name.Equals(prop, comparison))
                .First();
            }

            return propertyInfo;
        }
    }
}
