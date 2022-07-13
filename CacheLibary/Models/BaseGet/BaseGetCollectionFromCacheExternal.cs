using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models.BaseGet
{
  class BaseGetCollectionFromCacheExternal
  {
  }
  internal abstract class BaseGetCollectionFromCacheExternal<T, D, K> : BaseGetFromCache<ICollection<T>, K>, IBaseGetCollectionFromCacheExternal<T,D,K> where D : T, ICustomOptionDAO<T>, new()
  {
    internal BaseGetCollectionFromCacheExternal(IOptions options) : base(options)
    {
    }
    protected override async Task<ICollection<T>> GetFromPersistent(IKey<K> key)
    {
     return await PersistentManager.GetCollection<T, D, K>(key);
    }
    protected override void SaveToPersistent(IKey<K> key, ICollection<T> value)
    {
      PersistentManager.SaveCollection<T, D, K>(key, value, Options);
    }
  }
}
