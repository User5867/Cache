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
    protected override async Task<T> GetFromPersistent(IKey<K> singleKey)
    {
      return await PersistentManager.Get<T, D, K>(singleKey);
    }
    protected override void SaveToPersistent(IKey<K> singleKey, T singleValue)
    {
      PersistentManager.Save<T, D, K>(singleKey, singleValue, Options);
    }
  }
}
