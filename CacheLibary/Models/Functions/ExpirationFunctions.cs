using CacheLibary.DAOs;
using CacheLibary.Interfaces;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models.Functions
{
  internal class ExpirationFunctions
  {
    private static SQLiteAsyncConnection _db = PersistentCacheManager.Instance.GetDatabase();

    public static async void UpdateExperation(Key key)
    {
      Expiration expiration = key.Expiration;
      expiration.LastAccess = DateTime.UtcNow;
      await _db.UpdateAsync(expiration);
    }
    private static bool TryGetPersitentExperation(Key key, IOptions options, out Expiration expiration)
    {
      TimeSpan? expires = options.Expires?.PersitentExperation;
      if (expires.HasValue)
        expiration = new Expiration()
        {
          KeyId = key.Id,
          TotalExpiration = DateTime.UtcNow.Add(expires.Value),
          SlidingExpiration = options.Expires.PersistenSlidingExpiration,
          LastAccess = DateTime.UtcNow,
          Key = key
        };
      else
        expiration = null;
      return expires.HasValue;
    }

    internal static async Task CreateExpiration(Key key, IOptions options)
    {
      if (TryGetPersitentExperation(key, options, out Expiration expiration))
      {
        await _db.InsertOrReplaceWithChildrenAsync(expiration, true);
      }
    }

    internal static async void UpdateExperation<K>(IKey<K> key)
    {
      Key k = await KeyFunctions.GetKey(key);
      Expiration expiration = await _db.GetAsync<Expiration>(k.ExpirationId);
      expiration.LastAccess = DateTime.UtcNow;
      _ = await _db.UpdateAsync(expiration);
    }

    internal static async Task DeleteKeyAndExpiration()
    {
      List<Expiration> totalExpired = await _db.Table<Expiration>().Where(e => e.TotalExpiration != null).ToListAsync();
      _ = totalExpired.RemoveAll(e => e.TotalExpiration.Value > DateTime.UtcNow);
      IEnumerable<int> ids = totalExpired.Select(t => t.Id);
      List<Expiration> slidingExpired = await _db.Table<Expiration>().Where(e => e.SlidingExpiration != null && e.LastAccess != null && !ids.Contains(e.Id)).ToListAsync();
      _ = slidingExpired.RemoveAll(e => e.LastAccess.Value.Add(e.SlidingExpiration.Value) > DateTime.UtcNow);
      List<Expiration> expired = totalExpired;
      expired.AddRange(slidingExpired);
      IEnumerable<object> id = expired.Select(t => (object)t.Id);
      await _db.DeleteAllIdsAsync<Expiration>(id);
      IEnumerable<Key> keys = await _db.GetAllWithChildrenAsync<Key>(k => id.Contains(k.ExpirationId));
      foreach (Key key in keys)
      {
        key.Deleted = true;
        key.ExpirationId = -1;
      }
      await _db.UpdateAllAsync(keys);
    }

    internal static async Task DeleteExpired<D>(Type objectType) where D : new()
    {
      ICollection<Key> keys = await _db.Table<Key>().Where(k => k.Deleted).ToListAsync();
      await RemoveKeysForOtherTypes(objectType, keys);
      IEnumerable<int> keyIds = keys.Select(k => k.Id);
      IEnumerable<KeyValue> keyValues = await _db.Table<KeyValue>().Where(kv => keyIds.Contains(kv.KeyId)).ToListAsync();
      IEnumerable<int> valueIds = keyValues.Select(kv => kv.ValueId).Distinct();
      foreach (KeyValue kv in keyValues)
      {
        await _db.DeleteAsync(kv);
      }
      keyIds = (await _db.Table<KeyValue>().Where(kv => valueIds.Contains(kv.ValueId)).ToListAsync()).Select(kv => kv.KeyId);
      keys = await _db.Table<Key>().Where(k => keyIds.Contains(k.Id)).ToListAsync();
      await RemoveKeysForOtherTypes(objectType, keys);
      keyIds = keys.Select(k => k.Id);
      keyValues = await _db.Table<KeyValue>().Where(kv => keyIds.Contains(kv.KeyId)).ToListAsync();
      IEnumerable<int> vIds = keyValues.Select(kv => kv.ValueId).Distinct();
      IEnumerable<object> values = valueIds.ToList().Where(v => !vIds.Contains(v)).Select(v => (object)v);
      await _db.DeleteAllIdsAsync<D>(values);
    }

    private static async Task RemoveKeysForOtherTypes(Type objectType, ICollection<Key> keys)
    {
      foreach (Key key in keys)
      {
        await _db.GetChildrenAsync(key);
      }
      foreach (Key key in keys.ToList())
      {
        if (key.ObjectKey.ObjectType.FullName != objectType.FullName)
          _ = keys.Remove(key);
      }
    }
  }
}
