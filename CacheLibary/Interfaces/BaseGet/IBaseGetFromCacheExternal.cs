using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces
{
  interface IBaseGetFromCacheExternal<T, D> : IBaseGetFromCache<T>
  {
  }

  internal interface IBaseGetFromCacheExternal<T, D, K> : IBaseGetFromCacheExternal<T, D>, IBaseGetFromCache<T, K>
  {
  }
}
