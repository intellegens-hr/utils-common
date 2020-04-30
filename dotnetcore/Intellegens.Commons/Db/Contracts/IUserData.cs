using System;

namespace Intellegens.Commons.Db.Contracts
{
    public interface IUserData<TKey>
    {
        TKey GetUserId();
    }
}