using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces.CacheManager
{
  public interface ICacheManager
  {
    T GetCache<T>();
  }
}
