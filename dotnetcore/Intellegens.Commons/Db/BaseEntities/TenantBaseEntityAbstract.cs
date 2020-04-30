using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intellegens.Commons.Db.BaseEntities
{
    public abstract class TenantBaseEntityAbstract<UserEntity, TenantEntity> : BaseEntityAbstract<UserEntity>
        where UserEntity : class
        where TenantEntity : class
    {
        [ForeignKey(nameof(Tenant))]
        public Guid TenantId { get; set; }

        public TenantEntity Tenant { get; set; }
    }
}