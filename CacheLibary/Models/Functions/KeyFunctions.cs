using CacheLibary.DAOs;
using CacheLibary.Interfaces;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models.Functions
{
  internal class KeyFunctions
  {
    private static SQLiteAsyncConnection _db = PersistentCacheManager.Instance.GetDatabase();
    public static int GetHashcode<K>(IKey<K> key)
    {
      return key.GetHashCode();
    }
    public static async Task<Key> GetKey<K>(IKey<K> key)
    {
      int hashcode = GetHashcode(key);
      return await GetKeyByHashcode(hashcode, key);
    }
    public static async Task<Key> GetKeyByHashcode<K>(int hashcode, IKey<K> key)
    {
      Func<Key, bool> Equals = new Func<Key, bool>((d) =>
      {
        bool b = Key<K>.TryGetGenericKey(d.ObjectKey, out Key<K> genericKey);
        return b && key.Equals(genericKey);
      });
      return await HashFunctions.GetByHashcode<Key, IKey<K>>(hashcode, Equals);
    }
    public static int GetFirstFreeKeyIndex<K>(IKey<K> key)
    {
      int hashcode = GetHashcode(key);
      return GetFirstFreeKeyIndex(hashcode);
    }
    private static int GetFirstFreeKeyIndex(int hashcode)
    {
      return HashFunctions.GetFirstFreeIndex<Key>(hashcode);
    }
    public static async Task<Key> CreateAndGetKey<K>(IKey<K> key, IOptions options)
    {
      Key k = new Key() { ObjectKey = Key<K>.GetObjectKey(key), Hashcode = GetHashcode(key), Id = GetFirstFreeKeyIndex(key) };
      await ExpirationFunctions.CreateExpiration(k, options);
      return k;
    }
  }
}
