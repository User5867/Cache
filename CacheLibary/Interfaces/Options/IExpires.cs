using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces.Options
{
  internal interface IExpires
  {
    TimeSpan? MemoryExpiration { get; }
    TimeSpan? PersitentExperation { get; }
    TimeSpan? MemorySlidingExpiration { get; }
  }
}
