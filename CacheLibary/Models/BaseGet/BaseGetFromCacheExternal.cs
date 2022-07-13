using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models
{
  abstract class BaseGetFromCacheExternal<T, D, K> : BaseGetFromCache<T, K>, IBaseGetFromCacheExternal<T, D, K> where D : T, ICustomOptionDAO<T>, new()
  {
    internal BaseGetFromCacheExternal(IOptions options) : base(options)
    {
    }
    protected override async Task<T> GetFromPersistent(IKey<K> key)
    {
      return await PersistentManager.Get<T, D, K>(key);
    }
    protected override void SaveToPersistent(IKey<K> key, T value)
    {
      PersistentManager.Save<T, D, K>(key, value, Options);
    }
  }
}
