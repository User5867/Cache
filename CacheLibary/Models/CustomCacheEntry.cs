using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Models
{
  internal class CustomCacheEntry : ICacheEntry
  {
    public object Key { get; set; }

    public object Value { get; set; }
    public DateTimeOffset? AbsoluteExpiration { get; set; }
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }

    public IList<IChangeToken> ExpirationTokens { get; set; }

    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; set; }

    public CacheItemPriority Priority { get; set; }
    public long? Size { get; set; }

    [JsonConstructor]
    public CustomCacheEntry()
    {

    }
    internal CustomCacheEntry(ICacheEntry e)
    {
      Key = e.Key;
      Value = e.Value;
      AbsoluteExpiration = e.AbsoluteExpiration;
      AbsoluteExpirationRelativeToNow = e.AbsoluteExpirationRelativeToNow;
      SlidingExpiration = e.SlidingExpiration;
      ExpirationTokens = e.ExpirationTokens;
      PostEvictionCallbacks = e.PostEvictionCallbacks;
      Priority = e.Priority;
      Size = e.Size;
    }
    public void Dispose()
    {
      new NotImplementedException();
    }
  }
}
