using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core.CustomTypeProviders;

namespace Intellegens.Commons.Search
{
    public class DynamicLinqProvider : IDynamicLinkCustomTypeProvider
    {
        public HashSet<Type> GetCustomTypes()
        {
            HashSet<Type> types = new HashSet<Type>
            {
                typeof(EF),
                typeof(NpgsqlDbFunctionsExtensions),
                typeof(DbFunctionsExtensions)
            };
            return types;
        }

        public Type ResolveType(string typeName)
        {
            throw new NotImplementedException();
        }

        public Type ResolveTypeBySimpleName(string simpleTypeName)
        {
            throw new NotImplementedException();
        }
    }
}