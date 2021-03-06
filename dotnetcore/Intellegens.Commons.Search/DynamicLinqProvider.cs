﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Intellegens.Commons.Search
{
    /// <summary>
    /// Used by dynamic Linq library to enable support for Db functions inside dynamic queries.
    /// https://github.com/zzzprojects/System.Linq.Dynamic.Core/blob/master/src/System.Linq.Dynamic.Core/CustomTypeProviders/DefaultDynamicLinqCustomTypeProvider.cs
    /// </summary>
    public class DynamicLinqProvider : AbstractDynamicLinqCustomTypeProvider, IDynamicLinkCustomTypeProvider
    {
        private readonly IEnumerable<Type> additionalCustomTypes = Enumerable.Empty<Type>();

        public DynamicLinqProvider()
        {
        }

        public DynamicLinqProvider(IEnumerable<Type> additionalCustomTypes)
        {
            this.additionalCustomTypes = additionalCustomTypes;
        }

        public virtual HashSet<Type> GetCustomTypes()
        {
            HashSet<Type> types = new HashSet<Type>
            {
                typeof(EF),
                typeof(DbFunctionsExtensions)
            };

            foreach (var type in additionalCustomTypes)
                types.Add(type);

            return types;
        }

        public Dictionary<Type, List<MethodInfo>> GetExtensionMethods()
        {
            var types = GetCustomTypes();

            var list = new List<Tuple<Type, MethodInfo>>();

            foreach (var type in types)
            {
                var extensionMethods = type
                    .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(x => x.IsDefined(typeof(ExtensionAttribute), false));

                foreach (var method in extensionMethods)
                    list.Add(new Tuple<Type, MethodInfo>(method.GetParameters()[0].ParameterType, method));
            }

            return list
                .GroupBy(x => x.Item1, tuple => tuple.Item2)
                .ToDictionary(key => key.Key, methods => methods.ToList());
        }

        public Type ResolveType(string typeName)
            => ResolveType(AppDomain.CurrentDomain.GetAssemblies(), typeName);

        public Type ResolveTypeBySimpleName(string simpleTypeName)
            => ResolveTypeBySimpleName(AppDomain.CurrentDomain.GetAssemblies(), simpleTypeName);
    }
}