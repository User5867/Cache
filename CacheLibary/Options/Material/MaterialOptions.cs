using CacheLibary.Interfaces;
using CacheLibary.Interfaces.Options;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Options.Material
{
  internal class MaterialOptions : IOptions, IPriority, IExpires
  {
    public TimeSpan? MemoryExpiration => TimeSpan.FromHours(5);
    public TimeSpan? PersitentExperation => TimeSpan.FromHours(24);
    public CacheItemPriority Priority => CacheItemPriority.Low;
    public TimeSpan? MemorySlidingExpiration => TimeSpan.FromMinutes(10);

    public TimeSpan? PersistenSlidingExpiration => TimeSpan.FromHours(5);

    IPriority IOptions.Priority => this;
    IExpires IOptions.Expires => this;
    IUpdates IOptions.Updates => null;
  }
}
