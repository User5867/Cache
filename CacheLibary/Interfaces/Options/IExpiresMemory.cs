using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces.Options
{
  interface IExpiresMemory
  {
    TimeSpan? MemoryExpiration { get; }
    TimeSpan? MemorySlidingExpiration { get; }
  }
}
