using System;
using System.Collections.Generic;
using System.Linq;

namespace Intellegens.Commons.Search.FullTextSearch
{
    /// <summary>
    /// Use this attribute to flag Dto/entity properties as searchable by SearchService when using FullTextSearch
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FullTextSearchAttribute : Attribute
    {
        public List<string> TargetedProperties { get; private set; } = new List<string>();

        /// <summary>
        /// Flag property as searchable
        /// </summary>
        public FullTextSearchAttribute()
        {
        }

        /// <summary>
        /// If property is of complex type, specify which properties can be targeted
        /// </summary>
        /// <param name="propertiesCsv">Comma separated list of property names. E.g. "Title,Description"</param>
        public FullTextSearchAttribute(string propertiesCsv)
        {
            TargetedProperties = propertiesCsv.Split(',').ToList();
        }
    }
}