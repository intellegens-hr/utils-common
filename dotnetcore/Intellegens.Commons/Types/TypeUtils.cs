using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Intellegens.Commons.Types
{
    public static class TypeUtils
    {
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

        public static List<(PropertyInfo property, TAttribute attribute)> GetTypePropertiesWithAttribute<TAttribute>(this Type objectType)
            where TAttribute : Attribute
        {
            return objectType
                .GetProperties()
                .Where(x => x.GetCustomAttributes<TAttribute>().Any())
                .Select(x => (x, x.GetCustomAttribute<TAttribute>()))
                .ToList();
        }

        /// <summary>
        /// Returns all properties for type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<PropertyInfo> GetProperties(this Type type)
            => type.GetProperties().ToList();

        /// <summary>
        /// For all types which implement IEnumerable<T> - returns T.
        /// If type doesn't implement IEnumerable<T> - return type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type TransformToUnderlyingEnumerableTypeIfExists(this Type type)
        {
            if (type.GetIEnumerableGenericType() != null)
            {
                return type.GetIEnumerableGenericType();
            }

            return type;
        }

        /// <summary>
        /// For path in format complexProperty.nestedComplexProperty.Id, will return property information for each property in chain.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
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

        /// <summary>
        /// For path in format complexProperty.nestedComplexProperty.Id, will return property information for each property in chain.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public static IEnumerable<(string path, PropertyInfo propertyInfo, bool isCollectionType)> GetPropertyInfoPerPathSegment<T>(string propertyName, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where T : class
        {
            return GetPropertyInfoPerPathSegment(typeof(T), propertyName, comparison);
        }

        /// <summary>
        /// Check if given type implements IEnumerable
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsIEnumerableType(this Type type)
        {
            if (type == typeof(string))
                return false;

            return type.GetInterfaces()
                .Where(interfaceType => interfaceType.IsGenericType)
                .Where(interfaceType => interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Any();
        }

        /// <summary>
        /// Return base type if give type implements IEnumerable (ICollections, ILists, ...)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetIEnumerableGenericType(this Type type)
        {
            if (!type.IsIEnumerableType())
                return null;

            return type.GetInterfaces()
                .Where(interfaceType => interfaceType.IsGenericType)
                .Where(interfaceType => interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .FirstOrDefault()
                .GetGenericArguments()
                .FirstOrDefault();
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

        /// <summary>
        /// Determine whether a type is simple (String, Decimal, DateTime, etc)
        /// or complex (i.e. custom class with public properties and methods).
        /// </summary>
        /// <see cref="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive"/>
        public static bool IsSimpleType(
            this Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                new Type[] {
                typeof(String),
                typeof(Decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }
    }
}