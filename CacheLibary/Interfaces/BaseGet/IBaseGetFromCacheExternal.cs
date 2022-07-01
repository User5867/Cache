using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces
{
  interface IBaseGetFromCacheExternal<T, D>
  {
    IBaseGetFromCacheExternal<T, D, K> GetBaseGetFromCache<K>();
  }

  internal interface IBaseGetFromCacheExternal<T, D, K> : IBaseGetFromCache<T, K>, IBaseGetFromCacheExternal<T, D>
  {
  }
}
