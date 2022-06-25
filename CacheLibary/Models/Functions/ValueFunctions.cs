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
        return default;
      object o = k?.Values?.FirstOrDefault()?.Object;
      if (o != null)
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(o.ToString());
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
    public static async Task<D> GetValueByHashcode<D, T>(int hashcode, T value) where D : T
    {
      //int j = 0;
      //D k = await TryGetValueByIndex<D>(HashFunctions.GetIndexByHash(hashcode, j));
      //while (k != null || (k.Hashcode == hashcode && value.Equals(k.Object)))
      //{
      //  j++;
      //  k = await TryGetValueByIndex<D>(HashFunctions.GetIndexByHash(hashcode, j));
      //}
      //return k;
      throw new NotImplementedException();
    }

    internal static async Task<ICollection<T>> GetValuesByHashcode<T, K>(int hashcode, IKey<K> key)
    {
      Key k = await KeyFunctions.GetKeyByHashcode(hashcode, key);
      if (k == null)
        return default;
      ICollection<T> retVal = new List<T>();
      foreach (Value value in k.Values)
      {
        retVal.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value.Object.ToString()));
      }
      return retVal;
    }

    internal static async void SaveNewValues<K, T>(IKey<K> key, ICollection<T> values, IOptions options)
    {
      Key k = await CreateAndGetKey(key);
      ICollection<Value> v = await CreateAndGetValues(values);
      foreach (Value value in v)
      {
        AddKeyToValue(k, value);
      }
    }

    private static async Task<ICollection<Value>> CreateAndGetValues<T>(ICollection<T> values)
    {
      ICollection<Value> retVal = new List<Value>();
      foreach (T value in values)
      {
        retVal.Add(await CreateAndGetValue(value));
      }
      return retVal;
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
      KeyValue keyValue = new KeyValue() { KeyId = k.Id, ValueId = v.Id };
      _ = _db.InsertAsync(keyValue);
    }
    private static async Task<Key> CreateAndGetKey<K>(IKey<K> key)
    {

      Key k = new Key() { ObjectKey = Key<K>.GetObjectKey(key), Hashcode = KeyFunctions.GetHashcode(key), Id = await KeyFunctions.GetFirstFreeKeyIndex(key) };
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
        ValueId = value.Id,
        KeyId = key.Id
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
