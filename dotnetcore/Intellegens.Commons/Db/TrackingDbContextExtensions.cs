using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Intellegens.Commons.Db
{
    public static class TrackingDbContextExtensions
    {
        private static readonly Dictionary<string, List<Type>> typesWithPropertyCache = new Dictionary<string, List<Type>>();

        public static List<Type> GetDbContextEntitiesWithProperty<TContext>(this TContext dbContext, string propertyName)
            where TContext : DbContext
        {
            var dbContextType = dbContext.GetType();
            string dictionaryKey = $"{dbContextType.Name}_{propertyName}";
            
            if (!typesWithPropertyCache.ContainsKey(dictionaryKey))
            {
                var entityProps = dbContext
                    .GetType()
                    .GetProperties()
                    .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    .Select(x => x.PropertyType.GenericTypeArguments[0])
                    .Where(x => x.GetProperties().Select(x => x.Name).Contains(propertyName))
                    .ToList();

                typesWithPropertyCache[dictionaryKey] = entityProps;
            }

            return typesWithPropertyCache[dictionaryKey];
        }

        public enum ComparisonTypes
        {
            EQUAL, NOTEQUAL
        }

        public static void SetGlobalQueryFilter<TContext>(this TContext dbContext, ModelBuilder modelBuilder, string baseEntityPropertyName, object ExpectedValue, ComparisonTypes comparisonType = ComparisonTypes.EQUAL)
            where TContext : DbContext
        {
            var contextEntitiesFiltered = dbContext.GetDbContextEntitiesWithProperty(baseEntityPropertyName);

            foreach (var type in contextEntitiesFiltered)
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