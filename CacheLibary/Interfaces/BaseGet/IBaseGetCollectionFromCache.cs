using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces
{
  interface IBaseGetCollectionFromCache<T, K> : IBaseGetFromCache<ICollection<T>, K>
  {
  }
  interface IBaseGetCollectionFromCache<T> : IBaseGetCollectionFromCache<T, object>
  {

  }
}
