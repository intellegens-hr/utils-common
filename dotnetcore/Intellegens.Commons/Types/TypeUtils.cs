using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            where T : class
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

        public static Type TransformToUnderlyingEnumerableTypeIfExists(this Type type)
        {
            if (type.GetIEnumerableGenericType() != null)
            {
                return type.GetIEnumerableGenericType();
            }

            return type;
        }

        public static IEnumerable<(string path, PropertyInfo propertyInfo, bool isCollectionType)> GetPropertyInfoPerPathSegment(Type type, string propertyName, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            List<string> chain = propertyName.Split('.').ToList();
            PropertyInfo propertyInfo = null;

            for (int i = 0; i < chain.Count; i++)
            {
                var propertyType = propertyInfo?.PropertyType ?? type;
                propertyType = propertyInfo?.PropertyType?.GetIEnumerableGenericType() ?? propertyType;

                var pathSegment = chain[i];

                propertyInfo = GetProperties(propertyType)
                    .Where(x => x.Name.Equals(pathSegment, comparison))
                    .First();

                var isCollectionType = false;

                if (propertyInfo.PropertyType.GetIEnumerableGenericType() != null)
                {
                    var enumerableType = propertyInfo.PropertyType.GetIEnumerableGenericType();
                    isCollectionType = true;
                    propertyType = enumerableType;
                }

                var path = string.Join('.', chain.GetRange(0, i + 1));

                yield return (path, propertyInfo, isCollectionType);
            }
        }

        public static IEnumerable<(string path, PropertyInfo propertyInfo, bool isCollectionType)> GetPropertyInfoPerPathSegment<T>(string propertyName, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where T : class
        {
            return GetPropertyInfoPerPathSegment(typeof(T), propertyName, comparison);
        }

        /// <summary>
        /// Return base type if give type implements IEnumerable (ICollections, ILists, ...)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetIEnumerableGenericType(this Type type)
        {
            // string implements IEnumerable<char>
            if (type == typeof(string))
                return null;

            return type.GetInterfaces()
                .Where(interfaceType => interfaceType.IsGenericType)
                .Where(interfaceType => interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .FirstOrDefault()
                ?.GetGenericArguments()
                ?.FirstOrDefault();
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
            var propertyChainData = GetPropertyInfoPerPathSegment<T>(propertyName, comparison);
            return propertyChainData.Last().propertyInfo;
        }
    }
}