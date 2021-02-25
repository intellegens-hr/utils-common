using Intellegens.Commons.Db.BaseEntities;
using Intellegens.Commons.Db.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Intellegens.Commons.Db
{
    public static class TrackingDbContextExtensions
    {
        /// <summary>
        /// Cache used by "GetDbContextEntitiesWithProperty" method to avoid type parsing/reflection on each call
        /// </summary>
        private static readonly Dictionary<string, IEnumerable<Type>> typesWithPropertyCache = new Dictionary<string, IEnumerable<Type>>();

        /// <summary>
        /// In context's change tracker, goes through all entities and sets tracking data (user/date created/modified)
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="userData"></param>
        public static void SetEntityTrackingData<TKey>(this DbContext dbContext, IUserData<TKey> userData)
        {
            var entriesToCheck = dbContext.ChangeTracker
                .Entries()
                .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified)
                .Where(x => x.Entity is ITrackingEntity<TKey>)
                .Select(x => new
                {
                    x.State,
                    Entity = x.Entity as ITrackingEntity<TKey>
                });

            foreach (var entry in entriesToCheck)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.TimeCreated = DateTime.UtcNow;
                    entry.Entity.UserCreatedId = userData.GetUserId();
                }

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    entry.Entity.TimeUpdated = DateTime.UtcNow;
                    entry.Entity.UserUpdatedId = userData.GetUserId();
                }
            }
        }

        /// <summary>
        /// In provided DbContext, looks for all entites which have given property name. Useful when
        /// implementing global filters
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetDbContextEntitiesWithProperty<TContext>(this TContext dbContext, string propertyName)
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
                    .Where(x => x.GetProperties().Select(x => x.Name).Contains(propertyName));

                typesWithPropertyCache[dictionaryKey] = entityProps;
            }

            return typesWithPropertyCache[dictionaryKey];
        }

        public enum ComparisonTypes
        {
            EQUAL, NOTEQUAL
        }

        /// <summary>
        /// In given database context, sets global entity filter.
        /// For example, in multitenant environment this would be used to filter data by tenant:
        /// - baseEntityPropertyName would contain property all tenant related entities have - TenantId
        /// - expectedValue would contain current tenant Id
        /// - comparisonType would be set to EQUAL by default
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="modelBuilder"></param>
        /// <param name="baseEntityPropertyName"></param>
        /// <param name="ExpectedValue"></param>
        /// <param name="comparisonType"></param>
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