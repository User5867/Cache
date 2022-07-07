using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces
{
  interface IBaseGetCollectionFromCache<T, K> : IBaseGetCollectionFromCache<T>
  {
  }
  interface IBaseGetCollectionFromCache<T> : IBaseGetFromCache<ICollection<T>>
  {
  }
}
