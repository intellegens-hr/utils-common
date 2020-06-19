using Intellegens.Commons.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Intellegens.Commons.Search.FullTextSearch
{
    /// <summary>
    /// Used for building search paths for objects when using full-text search
    /// </summary>
    public static class FullTextSearchExtensions
    {
        private const int MAX_RECURSION_DEPTH = 10;

        /// <summary>
        /// For give type, parse possible search paths. It will be called recursivly
        /// </summary>
        /// <param name="type">Type to parse</param>
        /// <param name="propertiesToInclude">If any property is specified in this list, type won't be searched for FullText attributes - this list will be used</param>
        /// <param name="pathPrefix">prefix to add</param>
        /// <param name="depth">current recursion depth</param>
        /// <returns></returns>
        private static List<string> GetFullTextSearchPaths(Type type, List<string> propertiesToInclude = null, string pathPrefix = "", int depth = 0)
        {
            List<string> paths = new List<string>();
            propertiesToInclude ??= new List<string>();

            // find all properties in given type with FullTextSearchAttribute
            var fullTextAttributes = type
                .GetTypePropertiesWithAttribute<FullTextSearchAttribute>()
                .ToList();

            // if max depth has been reached - stop processing, path won't be returnes
            if (depth > MAX_RECURSION_DEPTH)
            {
            }
            // this will be recursion exit point - primitives are last possible path segment and must be returned
            else if (type.IsSimpleType())
            {
                paths.Add(pathPrefix);
            }
            // if this is complex object and no properties where specified in propertiesToInclude argument,
            // parse all properties with full text attribute
            else if (fullTextAttributes.Any() && !propertiesToInclude.Any())
            {
                foreach (var (property, attribute) in fullTextAttributes)
                {
                    string newPathPrefix = $"{property.Name}";
                    if (!string.IsNullOrEmpty(pathPrefix))
                        newPathPrefix = $"{pathPrefix}.{newPathPrefix}";

                    // FullTextAttribute has property TargetedProperties. If specified, only properties in that list should be taken, otherwise all
                    // which have FullTextAttribute
                    if (attribute.TargetedProperties.Any())
                    {
                        paths.AddRange(GetFullTextSearchPaths(property.PropertyType, attribute.TargetedProperties, newPathPrefix, depth: depth++));
                    }
                    else
                    {
                        paths.AddRange(GetFullTextSearchPaths(property.PropertyType, pathPrefix: newPathPrefix, depth: depth++));
                    }
                }
            }
            // In case type is enumerable, call this function again with underlying type
            else if (type.IsIEnumerableType())
            {
                GetFullTextSearchPaths(type.GetIEnumerableGenericType(), pathPrefix: pathPrefix, depth: depth);
            }
            // last possibility - if remaining path is empty, this is complex object and return it's string properties (default)
            // if remaining path is not empty, traverse further
            else
            {
                var props = type.GetProperties();

                props
                    .Where(x => x.PropertyType == typeof(string))
                    // if propertiesToInclude is specified - take only those properties
                    .Where(x => !propertiesToInclude.Any() || propertiesToInclude.Select(y => y.ToUpper()).Contains(x.Name.ToUpper()))
                    .ToList()
                    .ForEach(x =>
                    {
                        string newPathPrefix = x.Name;
                        if (!string.IsNullOrEmpty(pathPrefix))
                            newPathPrefix = $"{pathPrefix}.{newPathPrefix}";

                        paths.AddRange(GetFullTextSearchPaths(x.PropertyType, pathPrefix: $"{newPathPrefix}", depth: depth++));
                    });
            }

            return paths;
        }

        /// <summary>
        /// Parse specified type and return all paths that should be included in full-text search
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<string> GetFullTextSearchPaths<T>()
            where T : class, new()
            => GetFullTextSearchPaths(typeof(T));
    }
}