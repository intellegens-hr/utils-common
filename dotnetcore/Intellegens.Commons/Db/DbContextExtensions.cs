using Intellegens.Commons.Db.BaseEntities;
using Intellegens.Commons.Db.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Intellegens.Commons.Db
{
    public static class DbContextExtensions
    {
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
    }
}