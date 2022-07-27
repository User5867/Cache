using CacheLibary.DAOs;
using CacheLibary.Interfaces;
using SQLite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models.Functions
{
  internal class KeyFunctions
  {
    private static SQLiteAsyncConnection _db = PersistentCacheManager.Instance.GetDatabase();
    private const string GetExistingKeySelect = "select * from Key where ObjectKeyBlob = '{0}' and Deleted = false";
    private const string GetExistingKeysSelect = "select * from Key where ObjectKeyBlob in ({0}) and Deleted = false";
    private const string GetDeletedKeySelect = "select * from Key where ObjectKeyBlob = '{0}' and Deleted = true";
    private const string GetDeletedKeysSelect = "select * from Key where ObjectKeyBlob in ({0}) and Deleted = true";
    public static async Task<Key> GetExistingKey<K>(IKey<K> key)
    {
      string query = string.Format(GetExistingKeySelect, key.KeyBlob);
      return await _db.FindWithQueryAsync<Key>(query);
    }
    public static async Task<Key> GetDeletedKey<K>(IKey<K> key)
    {
      string query = string.Format(GetDeletedKeySelect, key.KeyBlob);
      return await _db.FindWithQueryAsync<Key>(query);
    }
    public static async Task<Key> CreateAndGetKey<K>(SQLiteConnection transaction, IKey<K> key, IOptions options)
    {
      Expiration e = ExpirationFunctions.GetExpiration(options);
      Key k = await GetExistingKey(key);
      if (k != null)
      {
        await ExpirationFunctions.UpdateExpiration(k, transaction);
        return null;
      }

      k = await GetDeletedKey(key);
      if (k == null)
      {
        k = new Key() { ObjectKeyBlob = key.KeyBlob };
        _ = transaction.Insert(k);
      }
      else
        k.Deleted = false;
      if (e == null)
        return k;
      e.KeyId = k.Id;
      _ = transaction.Insert(e);
      k.ExpirationId = e.Id;
      _ = transaction.Update(k);
      return k;
    }

    internal static async Task<IEnumerable<Key>> CreateAndGetKeys<K>(SQLiteConnection transaction, IEnumerable<IKey<K>> keys, IOptions options)
    {
      ConcurrentBag<Key> ks = new ConcurrentBag<Key>();
      Expiration e = ExpirationFunctions.GetExpiration(options);
      IEnumerable<Key> deletedKeys = await GetDeletedKeys(keys);
      IEnumerable<Key> existingKeys = await GetExistingKeys(keys);
      ILookup<string, Key> existingkeyLookUp = existingKeys.ToLookup(k => k.ObjectKeyBlob);
      _ = Parallel.ForEach(deletedKeys, k => k.Deleted = false);
      await ExpirationFunctions.UpdateExpirations(existingKeys, transaction);
      _ = transaction.UpdateAll(deletedKeys);
      ILookup<string, Key> deletedKeyLookUp = deletedKeys.ToLookup(k => k.ObjectKeyBlob);
      _ = Parallel.ForEach(keys, k =>
      {
        if (!deletedKeyLookUp.Contains(k.KeyBlob) && !existingkeyLookUp.Contains(k.KeyBlob))
        {
          ks.Add(new Key { ObjectKeyBlob = k.KeyBlob });
        }
      });
      if (ks.Count > 0)
      {
        _ = transaction.InsertAll(ks);
        _ = Parallel.ForEach(deletedKeys, dk => ks.Add(dk));
      }
      if (e == null)
        return ks;
      ConcurrentBag<Expiration> expirations = new ConcurrentBag<Expiration>();
      _ = Parallel.ForEach(ks, k => expirations.Add(new Expiration { KeyId = k.Id, LastAccess = e.LastAccess, SlidingExpiration = e.SlidingExpiration, TotalExpiration = e.TotalExpiration }));
      _ = transaction.InsertAll(expirations);
      ILookup<int, Key> lookupKey = ks.ToLookup(k => k.Id);
      _ = Parallel.ForEach(expirations, ex => lookupKey[ex.KeyId].First().ExpirationId = ex.Id);
      _ = transaction.UpdateAll(ks);
      System.Diagnostics.Debug.Write(18);
      return ks;
    }

    private static async Task<IEnumerable<Key>> GetExistingKeys<K>(IEnumerable<IKey<K>> keys)
    {
      string keyBlobs = "'" + string.Join("', '", keys.Select(k => k.KeyBlob)) + "'";
      string getDeletdKeys = string.Format(GetExistingKeysSelect, keyBlobs);
      return await _db.QueryAsync<Key>(getDeletdKeys);
    }
    private static async Task<IEnumerable<Key>> GetDeletedKeys<K>(IEnumerable<IKey<K>> keys)
    {
      string keyBlobs = "'" + string.Join("', '", keys.Select(k => k.KeyBlob)) + "'";
      string getDeletdKeys = string.Format(GetDeletedKeysSelect, keyBlobs);
      return await _db.QueryAsync<Key>(getDeletdKeys);
    }
  }
}
