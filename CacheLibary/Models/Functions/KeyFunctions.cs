using CacheLibary.DAOs;
using CacheLibary.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models.Functions
{
  internal class KeyFunctions
  {
    private static SQLiteAsyncConnection _db = PersistentCacheManager.Instance.GetDatabase();
    private const string GetDeletedKeySelect = "select * from Key where ObjectKeyBlob = '{0}' and Deleted = true";
    private const string GetDeletedKeysSelect = "select * from Key where ObjectKeyBlob in ({0}) and Deleted = true";
    public static async Task<Key> GetDeletedKey<K>(IKey<K> key)
    {
      string query = string.Format(GetDeletedKeySelect, key.KeyBlob);
      return await _db.FindWithQueryAsync<Key>(query);
    }
    //public static async Task<Key> GetKeyByHashcode<K>(int hashcode, IKey<K> key)
    //{
    //  Func<Key, bool> Equals = new Func<Key, bool>((d) =>
    //  {
    //    bool b = Key<K>.TryGetGenericKey(d.ObjectKey, out Key<K> genericKey);
    //    return b && key.Equals(genericKey);
    //  });
    //  return await HashFunctions.GetByHashcode<Key, IKey<K>>(hashcode, Equals);
    //}
    //public static int GetFirstFreeKeyIndex<K>(IKey<K> key)
    //{
    //  int hashcode = GetHashcode(key);
    //  return GetFirstFreeKeyIndex(hashcode);
    //}
    //private static int GetFirstFreeKeyIndex(int hashcode)
    //{
    //  return HashFunctions.GetFirstFreeIndex<Key>(hashcode);
    //}
    public static async Task<Key> CreateAndGetKey<K>(SQLiteConnection transaction, IKey<K> key, IOptions options)
    {
      Expiration e = ExpirationFunctions.GetExpiration(options);
      Key k = await GetDeletedKey(key);
      if (k == null)
      {
        k = new Key() { ObjectKeyBlob = key.KeyBlob };
        _ = transaction.Insert(k);
      }
      else
        k.Deleted = false;
      if(e == null)
        return k;
      e.KeyId = k.Id;
      _ = transaction.Insert(e);
      k.ExpirationId = e.Id;
      _ = transaction.Update(k);
      return k;
    }

    internal static async Task<IEnumerable<Key>> CreateAndGetKeys<K>(SQLiteConnection transaction, IEnumerable<IKey<K>> keys, IOptions options)
    {
      List<Key> ks = new List<Key>();
      Expiration e = ExpirationFunctions.GetExpiration(options);
      IEnumerable<Key> deletedKeys = await GetDeletedKeys(keys);
      foreach(Key k in deletedKeys)
      {
        k.Deleted = false;
      }
      _ = transaction.UpdateAll(deletedKeys);
      ILookup<string, Key> keyLookUp = deletedKeys.ToLookup(k => k.ObjectKeyBlob);
      foreach (IKey<K> key in keys)
      {
        if(!keyLookUp.Contains(key.KeyBlob))
          ks.Add(new Key { ObjectKeyBlob = key.KeyBlob });
      }
      _ = transaction.InsertAll(ks);
      ks.AddRange(deletedKeys);
      if (e == null)
        return ks;
      ICollection<Expiration> expirations = new List<Expiration>();
      foreach(Key k in ks)
      {
        expirations.Add(new Expiration { KeyId = k.Id, LastAccess = e.LastAccess, SlidingExpiration = e.SlidingExpiration, TotalExpiration = e.TotalExpiration });
      }
      _ = transaction.InsertAll(expirations);
      ILookup<int, Key> lookupKey = ks.ToLookup(k => k.Id);
      foreach(Expiration ex in expirations)
      {
        lookupKey[ex.KeyId].First().ExpirationId = ex.Id;
      }
      _ = transaction.UpdateAll(ks);
      return ks;
    }

    private static async Task<IEnumerable<Key>> GetDeletedKeys<K>(IEnumerable<IKey<K>> keys)
    {
      string keyBlobs = "'" + string.Join("', '", keys.Select(k => k.KeyBlob)) + "'";
      string getDeletdKeys = string.Format(GetDeletedKeysSelect, keyBlobs);
      return await _db.QueryAsync<Key>(getDeletdKeys);
    }
  }
}
