using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Interfaces
{
  internal interface IBaseGetFromCache<T, K>
  {
    Task<T> Get(K key);
  }
  internal interface IBaseGetFromCache<T>
  {
    IBaseGetFromCache<T, K> GetBaseGetFromCache<K>();
  }
}
