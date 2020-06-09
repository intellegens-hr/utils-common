using Intellegens.Commons.Db.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Intellegens.Commons.Db
{
    /// <summary>
    /// Base DbContext class which implements entity change tracking features.
    /// Basicly, it overrides SaveChanges methods to call "SetEntityTrackingData" extension method from "TrackingDbContextExtensions"
    /// before saving changes to database.
    /// </summary>
    /// <typeparam name="TKey">Id type</typeparam>
    public abstract class TrackingDbContextAbstract<TKey> : DbContext
    {
        protected IUserData<TKey> userData;

        public TrackingDbContextAbstract(IUserData<TKey> userData, DbContextOptions options) : base(options)
        {
            this.userData = userData;
        }

        protected TrackingDbContextAbstract(IUserData<TKey> userData)
        {
            this.userData = userData;
        }

        public override int SaveChanges()
        {
            this.SetEntityTrackingData(userData);
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.SetEntityTrackingData(userData);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.SetEntityTrackingData(userData);
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            this.SetEntityTrackingData(userData);
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}