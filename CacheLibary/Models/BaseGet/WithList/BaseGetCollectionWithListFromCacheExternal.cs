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
    protected override async Task GetFromPersistent()
    {
      SingleValue = await PersistentManager.Get<T, D, K>(SingleKey);
    }
    protected override void SaveOneToPersistent()
    {
      PersistentManager.Save<T, D, K>(SingleKey, SingleValue, Options);
    }
  }
}
