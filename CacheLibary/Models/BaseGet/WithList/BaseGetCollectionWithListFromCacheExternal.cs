using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models.BaseGet.WithList
{
  internal abstract class BaseGetCollectionWithListFromCacheExternal<T, D, K> : BaseGetCollectionWithListFromCache<T, K>, IBaseGetCollectionFromCacheExternal<T, D, K> where D : T, ICustomOptionDAO<T>, new()
  {
    internal BaseGetCollectionWithListFromCacheExternal(IOptions options) : base(options)
    {
    }
    protected override async Task<IEnumerable<T>> GetFromPersistent(IEnumerable<IKey<K>> keys)
    {
      return await PersistentManager.GetCollection<T, D, K>(keys);
    }

    protected override async Task SaveToPersistent(IKey<K> singleKey, T singleValue)
    {
      await PersistentManager.Save<T, D, K>(singleKey, singleValue, Options);
    }
  }
}
