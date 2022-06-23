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
  internal class ValueFunctions
  {
    private static SQLiteAsyncConnection _db = PersistentCacheManager.Instance.GetDatabase();

    public static async Task<T> GetValueByHashcode<T, K>(int hashcode, IKey<K> key)
    {
      Key k = await KeyFunctions.GetKeyByHashcode(hashcode, key);
      if (k == null)
      {
        return default;
      }
      if (k?.Values?.FirstOrDefault()?.Object is T retVal)
        return retVal;
      return default;
    }

    public static async Task<T> GetValueByHashcode<T, D, K>(int hashcode, IKey<K> key) where D : ICustomOptionDAO<T>, T, new()
    {
      Key k = await KeyFunctions.GetKeyByHashcode(hashcode, key);
      if (k == null)
      {
        return default;
      }
      return await GetValueByIndex<D>(k.Id);
    }

    private static async Task<T> GetValueByIndex<T>(int id) where T : new()
    {
      return await _db.GetAsync<T>(id);
    }

    public static async void SaveKeyForValue<K>(IKey<K> key, Value value, IOptions options)
    {
      List<Key> keys = value.Keys;
      if (keys.Find(k => k.ObjectKey == key) is Key ke && ke != null)
        ExpirationFunctions.UpdateExperation(ke, options);
      else if (await _db.GetAsync<Key>(key) is Key k && k != null)
        AddKeyToValue(k, value, options);
      else
        CreateKeyAndAddToValue(key, value, options);
    }
    public static int GetHashCode<T>(T value)
    {
      return value.ToString().GetHashCode();
    }
    public static async void SaveNewValue<T, K>(IKey<K> key, T value, IOptions options)
    {
      Key k = await CreateAndGetKey(key);
      Value v = await CreateAndGetValue(value);
      AddKeyToValue(k, v);
    }
    private static async Task<Value> CreateAndGetValue<T>(T value)
    {
      Value v = new Value() { Hashcode = GetHashCode(value), Object = value, Id = await GetFirstFreeValueIndex(GetHashCode(value)) };
      await _db.InsertWithChildrenAsync(v);
      return v;
    }
    public static async Task<Value> GetValueByHashcode<T>(int hashcode, T value)
    {
      int j = 0;
      Value k = await TryGetValueByIndex(HashFunctions.GetIndexByHash(hashcode, j));
      while (k != null || (k.Hashcode == hashcode && value.Equals(k.Object)))
      {
        j++;
        k = await TryGetValueByIndex(HashFunctions.GetIndexByHash(hashcode, j));
      }
      return k;
    }
    private static async Task<Value> TryGetValueByIndex(int index)
    {
      try
      {
        return await GetValueByIndex(index);
      }
      catch
      {
        return null;
      }
      
    }

    private static async Task<Value> GetValueByIndex(int index)
    {
      return await _db.GetWithChildrenAsync<Value>(index);
    }

    private static void AddKeyToValue(Key k, Value v)
    {
      KeyValue keyValue = new KeyValue() { Key = k.Id, Value = v.Id };
      _ = _db.InsertAsync(keyValue);
    }
    private static async Task<Key> CreateAndGetKey<K>(IKey<K> key)
    {
      Key k = new Key() { ObjectKey = key, Hashcode = KeyFunctions.GetHashcode(key), Id = await KeyFunctions.GetFirstFreeKeyIndex(key) };
      await _db.InsertWithChildrenAsync(k);
      return k;
    }
    private static async void CreateKeyAndAddToValue<K>(IKey<K> key, Value value, IOptions options)
    {
      Key k = await CreateAndGetKey(key);
      AddKeyToValue(k, value, options);
    }
    private static async void AddKeyToValue(Key key, Value value, IOptions options)
    {
      KeyValue keyValue = new KeyValue()
      {
        Value = value.Id,
        Key = key.Id
      };
      _ = await _db.InsertAsync(keyValue);
      ExpirationFunctions.UpdateExperation(key, options);
    }
    private static async Task<int> GetFirstFreeValueIndex(int hashcode)
    {
      int j = 0;
      int index;
      Value v;
      do
      {
        index = HashFunctions.GetIndexByHash(hashcode, j);
        v = await TryGetValueByIndex(index);
        j++;
      }
      while (v != null && v.Hashcode != PersistentCacheManager.Deletet);
      return index;
    }
  }
}
