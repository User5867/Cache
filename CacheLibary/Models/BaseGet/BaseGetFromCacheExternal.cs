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
    protected override async Task GetFromPersistent()
    {
      Value = await PersistentManager.Get<T, D, K>(Key);
    }
    protected override void SaveToPersistent()
    {
      PersistentManager.Save<T, D, K>(Key, Value, Options);
    }
  }
}
