using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces
{
  interface IBaseGetCollectionFromCacheExternal<T, D, K> : IBaseGetFromCacheExternal<ICollection<T>, D, K>
  {
  }
  interface IBaseGetCollectionFromCacheExternal<T, D> : IBaseGetCollectionFromCacheExternal<T, D, object>
  {

  }
}
