using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intellegens.Commons.Db.BaseEntities
{
    /// <summary>
    /// In case entites shouldn't be deleted, these are their possible states
    /// For example, in case entity is set to deleted - database should have global query filter
    /// which automatically filters these entites
    /// </summary>
    public enum StateEnum : byte
    {
        ACTIVE = 1,
        DELETED = 2
    }

    /// <summary>
    /// Basic change tracking interface all (tracked) entites should implement - directly or through other base classes
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface ITrackingEntity<TKey>
    {
        public TKey UserCreatedId { get; set; }
        public TKey UserUpdatedId { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeUpdated { get; set; }
    }

    /// <summary>
    /// Base entity change tracking class which implements tracking interface to avoid boilerplate code.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
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

    /// <summary>
    /// Base entity change tracking class which also contains foreign keys to UserCreated/UserModified.
    /// To avoid circular references, BaseEntityAbstract<TKey> will be inherited by User entity. 
    /// All other entites should inherit this class with User entity as second type parameter
    /// </summary>
    /// <typeparam name="TKey">Id type</typeparam>
    /// <typeparam name="TUserEntity">User entity model</typeparam>
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