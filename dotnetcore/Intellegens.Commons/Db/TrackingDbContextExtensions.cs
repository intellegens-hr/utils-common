using Intellegens.Commons.Db.BaseEntities;
using Intellegens.Commons.Db.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Intellegens.Commons.Db
{
    public static class TrackingDbContextExtensions
    {
        /// <summary>
        /// In context's change tracker, goes through all entities and sets tracking data (user/date created/modified)
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="userData"></param>
        public static void SetEntityTrackingData(this DbContext dbContext, IUserData userData)
        {
            var entriesToCheck = dbContext.ChangeTracker
                .Entries()
                .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified)
                .Where(x => x.Entity is BaseEntityAbstract)
                .Select(x => new
                {
                    x.State,
                    Entity = x.Entity as BaseEntityAbstract
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