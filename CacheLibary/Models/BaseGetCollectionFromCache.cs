using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Models
{
  internal abstract class BaseGetCollectionFromCache<T, K> : BaseGetFromCache<ICollection<T>, K>
  {
    internal BaseGetCollectionFromCache(IOptions options) : base(options)
    {
    }
  }
}
