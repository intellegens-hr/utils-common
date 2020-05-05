using Intellegens.Commons.Db.BaseEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Intellegens.Commons.Db
{
    public static class TrackingDbContextExtensions
    {
        private static readonly Dictionary<Type, List<Type>> typesCache = new Dictionary<Type, List<Type>>();

        public static List<Type> GetDbContextBaseEntityTypesCached<TContext, TKey>(this TContext dbContext)
            where TContext : TrackingDbContextAbstract<TKey>
        {
            var dbContextType = dbContext.GetType();

            if (!typesCache.ContainsKey(dbContextType))
            {
                var props = dbContext
                    .GetType()
                    .GetProperties()
                    .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    .Select(x => x.PropertyType.GenericTypeArguments[0])
                    .Where(x => x.BaseType == null || x.BaseType == typeof(BaseEntityAbstract<TKey>))
                    .ToList();

                typesCache[dbContextType] = props;
            }

            return typesCache[dbContextType];
        }

        public enum ComparisonTypes
        {
            EQUAL, NOTEQUAL
        }

        public static void SetGlobalQueryFilter<TContext, TKey>(this TContext dbContext, ModelBuilder modelBuilder, string baseEntityPropertyName, object ExpectedValue, ComparisonTypes comparisonType = ComparisonTypes.EQUAL)
            where TContext : TrackingDbContextAbstract<TKey>
        {
            foreach (var type in dbContext.GetDbContextBaseEntityTypesCached<TContext, TKey>())
            {
                var entityTypeBuilder = modelBuilder.Entity(type);

                var entityParameter = Expression.Parameter(type, "entity");
                var exprLeft = Expression.Property(entityParameter, baseEntityPropertyName);
                var exprRigt = Expression.Constant(ExpectedValue);

                BinaryExpression body = null;

                if (comparisonType == ComparisonTypes.EQUAL)
                    body = Expression.Equal(exprLeft, exprRigt);
                else if (comparisonType == ComparisonTypes.NOTEQUAL)
                    body = Expression.NotEqual(exprLeft, exprRigt);

                var delegateType = typeof(Func<,>).MakeGenericType(type, typeof(bool));
                var queryFilterLambda = Expression.Lambda(delegateType, body, entityParameter);

                entityTypeBuilder.HasQueryFilter(queryFilterLambda);
            }
        }
    }
}