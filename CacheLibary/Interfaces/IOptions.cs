using CacheLibary.Interfaces.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces
{
  internal interface IOptions
  {
    IPriority Priority { get; }
    IExpires Expires { get; }
    IUpdates Updates { get; }
  }
}
