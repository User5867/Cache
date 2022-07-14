using CacheLibary.DAOs;
using CacheLibary.Interfaces;
using CacheLibary.Interfaces.Options;
using SQLite;
using SQLiteNetExtensions.Extensions;
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
      //TimeSpan? expires = options.Expires?.PersitentExperation;
      if(options is IExpires expires)
      {
        DateTime? totalExpiration = null;
        if (expires.PersitentExperation.HasValue)
          totalExpiration = DateTime.UtcNow.Add(expires.PersitentExperation.Value);
        expiration = new Expiration()
        {
          KeyId = key.Id,
          TotalExpiration = totalExpiration,
          SlidingExpiration = options.Expires.PersistenSlidingExpiration,
          LastAccess = DateTime.UtcNow,
          Key = key
        };
      }
        
      else
        expiration = null;
      return expiration != null;
    }

    internal static void CreateExpiration(SQLiteConnection transaction, Key key, IOptions options)
    {
      if (TryGetPersitentExperation(key, options, out Expiration expiration))
      {
        transaction.InsertOrReplaceWithChildren(expiration, true);
      }
    }
    private static async Task<Expiration> TryGetExpiration(int id)
    {
      try
      {
        return await _db.GetAsync<Expiration>(id);
      }
      catch
      {
        return null;
      }
    }

    internal static async void UpdateExperation<K>(IKey<K> key)
    {
      Key k = await KeyFunctions.GetKey(key);
      if (k == null)
        return;
      Expiration expiration = await TryGetExpiration(k.ExpirationId);
      if (expiration == null)
        return;
      expiration.LastAccess = DateTime.UtcNow;
      _ = await _db.UpdateAsync(expiration);
    }

    internal static async Task DeleteKeyAndExpiration()
    {
      List<Expiration> expired = await GetExpired();
      IEnumerable<object> ids = expired.Select(t => (object)t.Id);
      await TryDeleteKeyAndExpirationWithIds(ids);

    }
    private static async Task TryDeleteKeyAndExpirationWithIds(IEnumerable<object> ids)
    {
      try
      {
        await DeleteKeyAndExpirationWithIds(ids);
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Fail(e.Message);
      }
    }
    private static async Task DeleteKeyAndExpirationWithIds(IEnumerable<object> ids)
    {
      await _db.RunInTransactionAsync(t =>
      {
        t.DeleteAllIds<Expiration>(ids);
        IEnumerable<Key> keys = t.GetAllWithChildren<Key>(k => ids.Contains(k.ExpirationId));
        SetAllKeysExpired(keys);
        _ = t.UpdateAll(keys);
      });
    }

    private static void SetAllKeysExpired(IEnumerable<Key> keys)
    {
      foreach (Key key in keys)
      {
        SetKeyExpired(key);
      }
    }

    private static void SetKeyExpired(Key key)
    {
      key.Deleted = true;
      key.ExpirationId = -1;
    }

    private static async Task<List<Expiration>> GetExpired()
    {
      List<Expiration> totalExpired = await GetTotalExpired();
      IEnumerable<int> ids = totalExpired.Select(t => t.Id);
      List<Expiration> slidingExpired = await GetSlidingExpired(ids);
      List<Expiration> expired = CombineExpired(totalExpired, slidingExpired);
      return expired;
    }

    private static List<Expiration> CombineExpired(List<Expiration> totalExpired, List<Expiration> slidingExpired)
    {
      List<Expiration> expired = totalExpired;
      expired.AddRange(slidingExpired);
      return expired;
    }

    private static async Task<List<Expiration>> GetSlidingExpired(IEnumerable<int> ids)
    {
      List<Expiration> slidingExpired = await _db.Table<Expiration>().Where(e => e.SlidingExpiration != null && e.LastAccess != null && !ids.Contains(e.Id)).ToListAsync();
      _ = slidingExpired.RemoveAll(e => e.LastAccess.Value.Add(e.SlidingExpiration.Value) > DateTime.UtcNow);
      return slidingExpired;
    }

    private static async Task<List<Expiration>> GetTotalExpired()
    {
      List<Expiration> totalExpired = await _db.Table<Expiration>().Where(e => e.TotalExpiration != null).ToListAsync();
      _ = totalExpired.RemoveAll(e => e.TotalExpiration.Value > DateTime.UtcNow);
      return totalExpired;
    }

    internal static async Task DeleteExpired<D>(Type objectType) where D : IHash, new()
    {
      ICollection<Key> keys = await GetExpiredKeys(objectType);
      IEnumerable<int> keyIds = GetKeyIds(keys);
      IEnumerable<int> valueIds = await ValueFunctions.GetValueIds(keyIds);
      SetAllKeyBlobsExpired(keys);
      await TryDeleteExpiredValues<D>(objectType, keys, keyIds, valueIds);
    }

    private static IEnumerable<int> GetKeyIds(ICollection<Key> keys)
    {
      return keys.Select(k => k.Id);
    }

    internal static async Task DeleteExpired(Type objectType)
    {
      ICollection<Key> keys = await GetExpiredKeys(objectType);
      IEnumerable<int> keyIds = GetKeyIds(keys);
      IEnumerable<int> valueIds = await ValueFunctions.GetValueIds(keyIds);
      SetAllKeyBlobsExpired(keys);
      await TryDeleteExpiredValues(keys, keyIds, valueIds);
    }
    private static async Task TryDeleteExpiredValues(ICollection<Key> keys, IEnumerable<int> keyIds, IEnumerable<int> valueIds)
    {
      try
      {
        await DeleteExpiredValues(keys, keyIds, valueIds);
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Fail(e.Message);
      }
    }
    private static async Task TryDeleteExpiredValues<D>(Type objectType, ICollection<Key> keys, IEnumerable<int> keyIds, IEnumerable<int> valueIds) where D : IHash, new()
    {
      try
      {
        await DeleteExpiredValues<D>(objectType, keys, keyIds, valueIds);
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Fail(e.Message);
      }
    }
    private static async Task DeleteExpiredValues(ICollection<Key> keys, IEnumerable<int> keyIds, IEnumerable<int> valueIds)
    {
      await _db.RunInTransactionAsync(t =>
      {
        _ = t.UpdateAll(keys);
        DeleteAllKeyValuesByKeyIds(t, keyIds);
        t.DeleteAllIds<Value>(GetObjectIEnumerable(valueIds));
      });
    }
    private static async Task DeleteExpiredValues<D>(Type objectType, ICollection<Key> keys, IEnumerable<int> keyIds, IEnumerable<int> valueIds) where D : IHash, new()
    {
      await _db.RunInTransactionAsync(async t =>
      {
        _ = t.UpdateAll(keys);
        DeleteAllKeyValuesByKeyIds(t, keyIds);
        IEnumerable<object> ids = await GetExpiredValueIdsAsObjects(objectType, valueIds);
        IEnumerable<D> values = await GetExpiredValues<D>(ids);
        t.UpdateAll(values);
      });
    }

    private static async Task<IEnumerable<D>> GetExpiredValues<D>(IEnumerable<object> ids) where D : IHash, new()
    {
      ICollection<D> values = new List<D>();
      foreach(object id in ids)
      {
        D value = await _db.GetAsync<D>(id);
        D expiredValue = new D
        {
          Deleted = true,
          Id = value.Id,
          Hashcode = value.Hashcode
        };
        values.Add(expiredValue);
      }
      return values;
    }

    private static async Task<IEnumerable<object>> GetExpiredValueIdsAsObjects(Type objectType, IEnumerable<int> valueIds)
    {
      IEnumerable<int> expiredvalueIds = await GetExpiredValueIds(objectType, valueIds);
      return GetObjectIEnumerable(expiredvalueIds);
    }

    private static IEnumerable<object> GetObjectIEnumerable(IEnumerable<int> expiredvalueIds)
    {
      return expiredvalueIds.Select(v => (object)v);
    }

    private static async Task<IEnumerable<int>> GetExpiredValueIds(Type objectType, IEnumerable<int> valueIds)
    {
      IEnumerable<int> notExpiredValueIds = await GetNotExpiredValueIds(objectType, valueIds);
      return valueIds.ToList().Where(v => !notExpiredValueIds.Contains(v));
    }

    private static async Task<IEnumerable<int>> GetNotExpiredValueIds(Type objectType, IEnumerable<int> valueIds)
    {
      IEnumerable<int> keyId = await GetExistingKeyIdsForValues(objectType, valueIds);
      return await ValueFunctions.GetValueIds(keyId);
    }

    private static async Task<IEnumerable<int>> GetExistingKeyIdsForValues(Type objectType, IEnumerable<int> valueIds)
    {
      IEnumerable<int> keyIds = await GetRemainingKeyIdsForValues(valueIds);
      ICollection<Key> keys = await GetKeysByIds(keyIds);
      await RemoveKeysForOtherTypes(objectType, keys);
      return keys.Select(k => k.Id);
    }

    private static async Task<ICollection<Key>> GetKeysByIds(IEnumerable<int> keyIds)
    {
      return await _db.Table<Key>().Where(k => keyIds.Contains(k.Id)).ToListAsync();
    }

    private static async Task<IEnumerable<int>> GetRemainingKeyIdsForValues(IEnumerable<int> valueIds)
    {
      return (await _db.Table<KeyValue>().Where(kv => valueIds.Contains(kv.ValueId)).ToListAsync()).Select(kv => kv.KeyId);
    }

    private static void DeleteAllKeyValuesByKeyIds(SQLiteConnection t, IEnumerable<int> keyIds)
    {
      foreach (int id in keyIds)
      {
        DeleteKeyValueByKeyId(t, id);
      }
    }

    private static void DeleteKeyValueByKeyId(SQLiteConnection t, int id)
    {
      _ = t.Query<KeyValue>("DELETE FROM [KeyValue] WHERE [KeyId] = " + id);
    }

    private static void SetAllKeyBlobsExpired(ICollection<Key> keys)
    {
      foreach (Key k in keys)
      {
        SetKeyBlobExpired(k);
      }
    }

    private static void SetKeyBlobExpired(Key k)
    {
      k.ObjectKeyBlob = null;
    }

    private static async Task<ICollection<Key>> GetExpiredKeys(Type objectType)
    {
      ICollection<Key> keys = await _db.Table<Key>().Where(k => k.Deleted && k.ObjectKeyBlob != null).ToListAsync();
      await RemoveKeysForOtherTypes(objectType, keys);
      return keys;
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
