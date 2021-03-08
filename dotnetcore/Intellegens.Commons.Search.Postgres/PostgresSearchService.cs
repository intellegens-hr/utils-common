using Microsoft.EntityFrameworkCore;
using System;

namespace Intellegens.Commons.Search
{
    /// <summary>
    /// Generic search services works on any IQueryable and provides simple (dynamic) filtering, search and ordering features on it
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class PostgresSearchService<T> : GenericSearchService<T> where T : class, new()
    {
        public PostgresSearchService() : base()
        {
        }

        protected override DynamicLinqProvider DynamicLinqProvider
            => new DynamicLinqProvider(new Type[] { typeof(NpgsqlDbFunctionsExtensions) });

        protected override string GetLikeFunctionName(Type filteredPropertyType)
        {
            // in case of string search, postgres uses ILIKE operator to do case insensitive search
            return (filteredPropertyType == typeof(string))
                ? "NpgsqlDbFunctionsExtensions.ILike"
                : base.GetLikeFunctionName(filteredPropertyType);
        }
    }
}