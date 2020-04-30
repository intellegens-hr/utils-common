using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intellegens.Commons.Db.BaseEntities
{
    public enum StateEnum : byte
    {
        ACTIVE = 1,
        DELETED = 2
    }

    public interface ITrackingEntity<TKey>
    {
        public TKey UserCreatedId { get; set; }
        public TKey UserUpdatedId { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeUpdated { get; set; }
    }

    public abstract class BaseEntityAbstract<TKey> : ITrackingEntity<TKey>
    {
        [Key]
        public virtual TKey Id { get; set; }

        public virtual TKey UserCreatedId { get; set; }
        public virtual TKey UserUpdatedId { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeUpdated { get; set; }

        [EnumDataType(typeof(StateEnum))]
        public StateEnum State { get; set; } = StateEnum.ACTIVE;
    }

    public abstract class BaseEntityAbstract<TKey, TUserEntity> : BaseEntityAbstract<TKey>
        where TUserEntity : class
    {
        [ForeignKey(nameof(UserCreated))]
        public override TKey UserCreatedId { get; set; }

        [ForeignKey(nameof(UserUpdated))]
        public override TKey UserUpdatedId { get; set; }

        public TUserEntity UserCreated { get; set; }
        public TUserEntity UserUpdated { get; set; }
    }
}