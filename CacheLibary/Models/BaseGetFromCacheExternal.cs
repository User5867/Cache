using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models
{
  abstract class BaseGetFromCacheExternal<T, D, K> : BaseGetFromCache<T, D, K>, IBaseGetFromCacheExternal<T, D, K> where D : T, ICustomOptionDAO<T>, new()
  {
    internal BaseGetFromCacheExternal(IOptions options) : base(options)
    {
    }

    public abstract IBaseGetFromCacheExternal<T, D, K1> GetBaseGetFromCache<K1>();
  }
}
