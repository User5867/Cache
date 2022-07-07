using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces
{
  interface IBaseGetCollectionFromCacheExternal<T, D, K> : IBaseGetCollectionFromCacheExternal<T, D>
  {
  }
  interface IBaseGetCollectionFromCacheExternal<T, D> : IBaseGetFromCacheExternal<ICollection<T>, D>
  {
  }
}
