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
    public static async Task<Key> GetKeyByHashcode<K>(int hashcode, IKey<K> key)
    {
      int j = 0;
      Key k = await TryGetKeyByIndex(HashFunctions.GetIndexByHash(hashcode, j));
      while (k != null && !(k.Hashcode == hashcode && key.Equals(k.ObjectKey)))
      {
        j++;
        k = await TryGetKeyByIndex(HashFunctions.GetIndexByHash(hashcode, j));
      }
      return k;
    }
    private static async Task<Key> TryGetKeyByIndex(int v)
    {
      try
      {
        return await GetKeyByIndex(v);
      }
      catch
      {
        return null;
      }
      
    }
    private static async Task<Key> GetKeyByIndex(int v)
    {
      return await _db.GetWithChildrenAsync<Key>(v);
    }
    public static async Task<int> GetFirstFreeKeyIndex<K>(IKey<K> key)
    {
      return await GetFirstFreeKeyIndex(GetHashcode(key));
    }
    private static async Task<int> GetFirstFreeKeyIndex(int hashcode)
    {
      int j = 0;
      int index;
      Key k;
      do
      {
        index = HashFunctions.GetIndexByHash(hashcode, j);
        k = await TryGetKeyByIndex(index);
        j++;
      }
      while (k != null && k.Hashcode != PersistentCacheManager.Deletet);
      return index;
    }
    
  }
}
