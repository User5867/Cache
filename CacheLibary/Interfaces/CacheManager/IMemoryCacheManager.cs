using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Interfaces.CacheManager
{
  internal interface IMemoryCacheManager
  {
    T Get<T, K>(IKey<K> key);
    void Save<T, K>(IKey<K> key, T value, IOptions options);
  }
}
