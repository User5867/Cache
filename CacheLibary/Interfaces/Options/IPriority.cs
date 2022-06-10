using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces.Options
{
  internal interface IPriority
  {
    CacheItemPriority Priority { get; }
  }
}
