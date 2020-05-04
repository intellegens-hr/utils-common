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

    public abstract class BaseEntityAbstract
    {
        [Key]
        public Guid Id { get; set; }

        public virtual Guid UserCreatedId { get; set; }
        public virtual Guid UserUpdatedId { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeUpdated { get; set; }

        [EnumDataType(typeof(StateEnum))]
        public StateEnum State { get; set; } = StateEnum.ACTIVE;
    }

    public abstract class BaseEntityAbstract<UserEntity> : BaseEntityAbstract
        where UserEntity : class
    {
        [ForeignKey(nameof(UserCreated))]
        public override Guid UserCreatedId { get; set; }

        [ForeignKey(nameof(UserUpdated))]
        public override Guid UserUpdatedId { get; set; }

        public UserEntity UserCreated { get; set; }
        public UserEntity UserUpdated { get; set; }
    }
}