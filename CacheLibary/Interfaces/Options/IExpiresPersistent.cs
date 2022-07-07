using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces.Options
{
  interface IExpiresPersistent
  {
    TimeSpan? PersitentExperation { get; }
    TimeSpan? PersistenSlidingExpiration { get; }
  }
}
