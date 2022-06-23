using CacheLibary.DAOs;
using CacheLibary.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Models.Functions
{
  internal class ExpirationFunctions
  {
    private static SQLiteAsyncConnection _db = PersistentCacheManager.Instance.GetDatabase();

    public static void UpdateExperation(Key key, IOptions options)
    {
      if (TryGetPersitentExperation(key, options, out Expiration expiration))
      {
        _ = _db.UpdateAsync(expiration);
      }
    }
    private static bool TryGetPersitentExperation(Key key, IOptions options, out Expiration expiration)
    {
      TimeSpan? expires = options.Expires?.PersitentExperation;
      if (expires.HasValue)
        expiration = new Expiration()
        {
          Key = key.ObjectKey,
          Experation = DateTime.UtcNow.Add(options.Expires.PersitentExperation.Value)
        };
      else
        expiration = null;
      return expires.HasValue;
    }
  }
}
