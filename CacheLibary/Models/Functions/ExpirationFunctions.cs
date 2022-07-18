using CacheLibary.DAOs;
using CacheLibary.Interfaces;
using CacheLibary.Interfaces.Options;
using SQLite;
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
      //Expiration expiration = key.Expiration;
      //expiration.LastAccess = DateTime.UtcNow;
      //await _db.UpdateAsync(expiration);
    }

    internal static Expiration GetExpiration(IOptions options)
    {
      Expiration expiration;
      if (options is IExpires expires)
      {
        DateTime? totalExpiration = null;
        if (expires.PersitentExperation.HasValue)
          totalExpiration = DateTime.UtcNow.Add(expires.PersitentExperation.Value);
        expiration = new Expiration()
        {
          TotalExpiration = totalExpiration,
          SlidingExpiration = options.Expires.PersistenSlidingExpiration,
          LastAccess = DateTime.UtcNow,
        };
      }
      else
        expiration = null;
      return expiration;
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
      //Key k = await KeyFunctions.GetKey(key);
      //if (k == null)
      //  return;
      //Expiration expiration = await TryGetExpiration(k.ExpirationId);
      //if (expiration == null)
      //  return;
      //expiration.LastAccess = DateTime.UtcNow;
      //_ = await _db.UpdateAsync(expiration);
    }
    private const string GetExpired = "select Id from Expiration where TotalExpiration != 0 and TotalExpiration < {0} UNION select Id from Expiration where SlidingExpiration != 0 and SlidingExpiration + LastAccess < {0}";
    private const string SetKeysDeleted = "update Key set Deleted = true where ExpirationId in ( {0} )";
    private const string DeleteExpirations = "delete from Expiration where Id in ( {0} )";
    internal static async Task DeleteKeyAndExpiration()
    {
      string query = string.Format(GetExpired, DateTime.UtcNow.Ticks);
      IEnumerable<int> expiredIds = await _db.QueryScalarsAsync<int>(query);
      if (!expiredIds.Any())
        return;
      string expiredString = string.Join(", ", expiredIds);
      string updateQuery = string.Format(SetKeysDeleted, expiredString);
      string deleteQuery = string.Format(DeleteExpirations, expiredString);
      await _db.RunInTransactionAsync(t =>
      {
        _ = t.Execute(updateQuery);
        _ = t.Execute(deleteQuery);
      });
    }
    //private static async Task TryDeleteKeyAndExpirationWithIds(IEnumerable<object> ids)
    //{
    //  try
    //  {
    //    await DeleteKeyAndExpirationWithIds(ids);
    //  }
    //  catch (Exception e)
    //  {
    //    System.Diagnostics.Debug.Fail(e.Message);
    //  }
    //}
    //private static async Task DeleteKeyAndExpirationWithIds(IEnumerable<object> ids)
    //{
    //  await _db.RunInTransactionAsync(t =>
    //  {
    //    t.DeleteAllIds<Expiration>(ids);
    //    IEnumerable<Key> keys = t.GetAllWithChildren<Key>(k => ids.Contains(k.ExpirationId));
    //    SetAllKeysExpired(keys);
    //    _ = t.UpdateAll(keys);
    //  });
    //}

    //private static void SetAllKeysExpired(IEnumerable<Key> keys)
    //{
    //  foreach (Key key in keys)
    //  {
    //    SetKeyExpired(key);
    //  }
    //}

    //private static void SetKeyExpired(Key key)
    //{
    //  key.Deleted = true;
    //  key.ExpirationId = -1;
    //}

    //private static async Task<List<Expiration>> GetExpired()
    //{
    //  List<Expiration> totalExpired = await GetTotalExpired();
    //  IEnumerable<int> ids = totalExpired.Select(t => t.Id);
    //  List<Expiration> slidingExpired = await GetSlidingExpired(ids);
    //  List<Expiration> expired = CombineExpired(totalExpired, slidingExpired);
    //  return expired;
    //}

    private const string GetDeletedKeyIds = "select Id from Key where Deleted = true and ObjectKeyBlob Like '%{0}%'";
    private const string GetToDeleteValueIds = "select d.Id from Key as k Join (select * from KeyValue join (select Id from {0}) on Id = ValueId) as d on k.Id = KeyId where k.Id in ( {1} )";
    private const string DeleteKeys = "delete from Key where Id in ( {0} )";
    private const string DeleteKeyValues = "delete from KeyValue where KeyId in ( {0} )";
    private const string DeleteDaoValues = "delete from {0} where Id in (select Id from {0} where Id in ( {1} ) and Id not in (select distinct Id from (((select Id from MaterialDAO where Id in ( {1} )) join (select * from KeyValue) on Id = KeyId) join (Select Id as kId from Key where ObjectKeyBlob like '%{2}%') on kId = KeyId)))";
    internal static async Task DeleteExpired<D>(Type objectType) where D : ICustomOptionDAO, new()
    {
      string s = objectType.AssemblyQualifiedName;
      string deletedKeysQuery = string.Format(GetDeletedKeyIds, s);
      IEnumerable<int> deletedKeyIds = await _db.QueryScalarsAsync<int>(deletedKeysQuery);
      if (!deletedKeyIds.Any())
        return;
      string deletedIds = string.Join(", ", deletedKeyIds);
      string deleteKeys = string.Format(DeleteKeys, deletedIds);
      string deleteKeyValues = string.Format(DeleteKeyValues, deletedIds);
      TableMapping mapping = await _db.GetMappingAsync<D>();
      string getToDeletedValues = string.Format(GetToDeleteValueIds, mapping.TableName, deletedIds);
      IEnumerable<int> toDeleteValueIds = await _db.QueryScalarsAsync<int>(getToDeletedValues);
      string toDeleteValues = string.Join(", ", toDeleteValueIds);
      string deleteValues = string.Format(DeleteDaoValues, mapping.TableName, toDeleteValues, s);
      await _db.RunInTransactionAsync(t =>
      {
        int i = t.Execute(deleteKeys);
        i = t.Execute(deleteKeyValues);
        i = t.Execute(deleteValues);
        //t.Execute();
      });
      //ICollection<Key> keys = await GetExpiredKeys(objectType);
      //IEnumerable<int> keyIds = GetKeyIds(keys);
      //IEnumerable<int> valueIds = await ValueFunctions.GetValueIds(keyIds);
      //SetAllKeyBlobsExpired(keys);
      //await TryDeleteExpiredValues<D>(objectType, keys, keyIds, valueIds);
    }

    internal static async Task DeleteExpired(Type objectType)
    {
      //ICollection<Key> keys = await GetExpiredKeys(objectType);
      //IEnumerable<int> keyIds = GetKeyIds(keys);
      //IEnumerable<int> valueIds = await ValueFunctions.GetValueIds(keyIds);
      //SetAllKeyBlobsExpired(keys);
      //await TryDeleteExpiredValues(keys, keyIds, valueIds);
    }
    //private static async Task TryDeleteExpiredValues(ICollection<Key> keys, IEnumerable<int> keyIds, IEnumerable<int> valueIds)
    //{
    //  try
    //  {
    //    await DeleteExpiredValues(keys, keyIds, valueIds);
    //  }
    //  catch (Exception e)
    //  {
    //    System.Diagnostics.Debug.Fail(e.Message);
    //  }
    //}
    //private static async Task TryDeleteExpiredValues<D>(Type objectType, ICollection<Key> keys, IEnumerable<int> keyIds, IEnumerable<int> valueIds) where D : ICustomOptionDAO, new()
    //{
    //  try
    //  {
    //    await DeleteExpiredValues<D>(objectType, keys, keyIds, valueIds);
    //  }
    //  catch (Exception e)
    //  {
    //    System.Diagnostics.Debug.Fail(e.Message);
    //  }
    //}
    private static async Task DeleteExpiredValues(ICollection<Key> keys, IEnumerable<int> keyIds, IEnumerable<int> valueIds)
    {
      await _db.RunInTransactionAsync(t =>
      {
        _ = t.UpdateAll(keys);
        DeleteAllKeyValuesByKeyIds(t, keyIds);
        //t.DeleteAllIds<Value>(GetObjectIEnumerable(valueIds));
      });
    }
    //private static async Task DeleteExpiredValues<D>(Type objectType, ICollection<Key> keys, IEnumerable<int> keyIds, IEnumerable<int> valueIds) where D : ICustomOptionDAO, new()
    //{
    //  await _db.RunInTransactionAsync(async t =>
    //  {
    //    _ = t.UpdateAll(keys);
    //    DeleteAllKeyValuesByKeyIds(t, keyIds);
    //    IEnumerable<object> ids = await GetExpiredValueIdsAsObjects(objectType, valueIds);
    //    IEnumerable<D> values = await GetExpiredValues<D>(ids);
    //    t.UpdateAll(values);
    //  });
    //}

    //private static async Task<IEnumerable<D>> GetExpiredValues<D>(IEnumerable<object> ids) where D : ICustomOptionDAO, new()
    //{
    //  ICollection<D> values = new List<D>();
    //  foreach(object id in ids)
    //  {
    //    D value = await _db.GetAsync<D>(id);
    //    D expiredValue = new D
    //    {
    //      Deleted = true,
    //      Id = value.Id,
    //      Hashcode = value.Hashcode
    //    };
    //    values.Add(expiredValue);
    //  }
    //  return values;
    //}

    //private static async Task<IEnumerable<object>> GetExpiredValueIdsAsObjects(Type objectType, IEnumerable<int> valueIds)
    //{
    //  IEnumerable<int> expiredvalueIds = await GetExpiredValueIds(objectType, valueIds);
    //  return GetObjectIEnumerable(expiredvalueIds);
    //}

    private static IEnumerable<object> GetObjectIEnumerable(IEnumerable<int> expiredvalueIds)
    {
      return expiredvalueIds.Select(v => (object)v);
    }

    //private static async Task<IEnumerable<int>> GetExpiredValueIds(Type objectType, IEnumerable<int> valueIds)
    //{
    //  IEnumerable<int> notExpiredValueIds = await GetNotExpiredValueIds(objectType, valueIds);
    //  return valueIds.ToList().Where(v => !notExpiredValueIds.Contains(v));
    //}

    //private static async Task<IEnumerable<int>> GetNotExpiredValueIds(Type objectType, IEnumerable<int> valueIds)
    //{
    //  IEnumerable<int> keyId = await GetExistingKeyIdsForValues(objectType, valueIds);
    //  return await ValueFunctions.GetValueIds(keyId);
    //}

    //private static async Task<IEnumerable<int>> GetExistingKeyIdsForValues(Type objectType, IEnumerable<int> valueIds)
    //{
    //  IEnumerable<int> keyIds = await GetRemainingKeyIdsForValues(valueIds);
    //  ICollection<Key> keys = await GetKeysByIds(keyIds);
    //  await RemoveKeysForOtherTypes(objectType, keys);
    //  return keys.Select(k => k.Id);
    //}

    //private static async Task<ICollection<Key>> GetKeysByIds(IEnumerable<string> keyIds)
    //{
    //  return await _db.Table<Key>().Where(k => keyIds.Contains(k.ObjectKeyBlob)).ToListAsync();
    //}

    //private static async Task<IEnumerable<int>> GetRemainingKeyIdsForValues(IEnumerable<int> valueIds)
    //{
    //  return (await _db.Table<KeyValue>().Where(kv => valueIds.Contains(kv.ValueId)).ToListAsync()).Select(kv => kv.KeyId);
    //}

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

    //private static async Task<ICollection<Key>> GetExpiredKeys(Type objectType)
    //{
    //  ICollection<Key> keys = await _db.Table<Key>().Where(k => k.Deleted && k.ObjectKeyBlob != null).ToListAsync();
    //  await RemoveKeysForOtherTypes(objectType, keys);
    //  return keys;
    //}

    private static async Task RemoveKeysForOtherTypes(Type objectType, ICollection<Key> keys)
    {
      //foreach (Key key in keys)
      //{
      //  await _db.GetAsync(key);
      //}
      //foreach (Key key in keys.ToList())
      //{
      //  if (key.ObjectKey.ObjectType.FullName != objectType.FullName)
      //    _ = keys.Remove(key);
      //}
    }
  }
}
