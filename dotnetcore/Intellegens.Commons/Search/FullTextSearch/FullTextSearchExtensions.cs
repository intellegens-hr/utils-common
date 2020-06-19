using Intellegens.Commons.Types;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Intellegens.Commons.Search.FullTextSearch
{
    public static class FullTextSearchExtensions
    {
        private const int MAX_DEPTH = 10;

        /// <summary>
        /// For give type, parse possible search paths. It will be called recursivly
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertiesToInclude"></param>
        /// <param name="pathPrefix"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private static List<string> GetFullTextSearchPaths(Type type, List<string> propertiesToInclude = null, string pathPrefix = "", int depth = 0)
        {
            List<string> paths = new List<string>();

            propertiesToInclude ??= new List<string>();
            var fullTextAttributes = type
                .GetTypePropertiesWithAttribute<FullTextSearchAttribute>()
                //.Where(x => !propertiesToInclude.Any() || propertiesToInclude.Contains(x.property.Name))
                .ToList();

            // if max depth has been reached - stop processing
            if (depth > MAX_DEPTH)
            {
            }
            // this will be recursion exit point - primitives are last possible path segment and must be returned
            else if (type.IsSimpleType())
            {
                paths.Add(pathPrefix);
            }
            // if object implements IFullTextSearch - parse only paths it specifies, unless this is last path segment.
            // in that case, process only given path
            else if (fullTextAttributes.Any() && !propertiesToInclude.Any())
            {
                foreach (var (property, attribute) in fullTextAttributes)
                {
                    string newPathPrefix = $"{property.Name}";
                    if (!string.IsNullOrEmpty(pathPrefix))
                        newPathPrefix = $"{pathPrefix}.{newPathPrefix}";

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
            // In case type is enumerable, call again with underlying type
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
                    .Where(x => !propertiesToInclude.Any() || propertiesToInclude.Contains(x.Name))
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

        public static List<string> GetFullTextSearchPaths<T>()
            where T : class, new()
            => GetFullTextSearchPaths(typeof(T));
    }
}