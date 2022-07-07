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
    protected override async Task GetFromPersistent()
    {
      Value = await PersistentManager.GetCollection<T, D, K>(Key);
    }
    protected override void SaveToPersistent()
    {
      PersistentManager.SaveCollection<T, D, K>(Key, Value, Options);
    }
  }
}
