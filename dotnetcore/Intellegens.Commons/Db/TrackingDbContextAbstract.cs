using Intellegens.Commons.Db.BaseEntities;
using Intellegens.Commons.Db.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Intellegens.Commons.Db
{
    public abstract class TrackingDbContextAbstract : DbContext
    {
        protected IUserData userData;

        public TrackingDbContextAbstract(IUserData userData, DbContextOptions options) : base(options)
        {
            this.userData = userData;
        }

        protected TrackingDbContextAbstract(IUserData userData)
        {
            this.userData = userData;
        }

        private void SetEntityTrackingData()
        {
            var entriesToCheck = base.ChangeTracker
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

        public override int SaveChanges()
        {
            SetEntityTrackingData();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            SetEntityTrackingData();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetEntityTrackingData();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            SetEntityTrackingData();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}