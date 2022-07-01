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

    internal static async Task<T> GetValue<T, K>(IKey<K> key)
    {
      Key k = await KeyFunctions.GetKey(key);
      if (k == null)
        return default;
      object o = k?.Values?.FirstOrDefault()?.Object;
      if (o != null)
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(o.ToString());
      return default;
    }
    internal static async Task<D> GetValue<T, D, K>(IKey<K> key) where D : ICustomOptionDAO<T>, T, new()
    {
      Key k = await KeyFunctions.GetKey(key);
      if (k == null)
        return default;
      int? valueId = await GetValueId(k.Id);
      if (!valueId.HasValue)
        return default;
      return await GetValueByIndex<D>(valueId.Value);
    }

    private static async Task<int?> GetValueId(int keyId)
    {
      return (await _db.Table<KeyValue>().Where(kv => kv.KeyId == keyId).FirstOrDefaultAsync()).ValueId;
    }

    private static async Task<ICollection<int>> GetValueIds(int keyId)
    {
      return (await _db.Table<KeyValue>().Where(kv => kv.KeyId == keyId).ToListAsync()).Select(kv => kv.ValueId).ToList();
    }

    private static async Task<T> GetValueByIndex<T>(int id) where T : new()
    {
      return await _db.GetWithChildrenAsync<T>(id);
    }
    public static async void SaveNewValue<T, K>(IKey<K> key, T value, IOptions options)
    {
      Key k = await KeyFunctions.CreateAndGetKey(key, options);
      Value v = await CreateAndGetValue(value);
      AddKeyToValue(k, v);
    }

    private static async Task<Value> CreateAndGetValue<T>(T value)
    {
      Value v = new Value() { Object = value };
      await _db.InsertWithChildrenAsync(v);
      return v;
    }

    private static async Task<D> CreateAndGetValue<T, D>(T value) where D : ICustomOptionDAO<T>, T, new()
    {
      D v = new D().CreateInstance<D>(value);
      v.Id = HashFunctions.GetFirstFreeIndex<D>(v.Hashcode);
      await _db.InsertWithChildrenAsync(v);
      return v;
    }
    internal static async Task<ICollection<T>> GetValues<T, K>(IKey<K> key)
    {
      Key k = await KeyFunctions.GetKey(key);
      if (k == null)
        return default;
      ICollection<T> retVal = new List<T>();
      foreach (Value value in k.Values)
      {
        retVal.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value.ObjectBlob));
      }
      return retVal;
    }

    internal static async void SaveNewValues<K, T>(IKey<K> key, ICollection<T> values, IOptions options)
    {
      Key k = await KeyFunctions.CreateAndGetKey(key, options);
      ICollection<Value> v = await CreateAndGetValues(values);
      foreach (Value value in v)
      {
        AddKeyToValue(k, value);
      }
    }

    internal static async void SaveNewValue<T, D, K>(IKey<K> key, T value, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      Key k = await KeyFunctions.CreateAndGetKey(key, options);
      SaveNewValue<T, D, K>(k, value);
    }

    private static async void SaveNewValue<T, D, K>(Key key, T value) where D : ICustomOptionDAO<T>, T, new()
    {
      D v = await HashFunctions.GetByHashcode<D, T>(new D().CreateInstance<D>(value).Hashcode, value);
      if (v == null)
        v = await CreateAndGetValue<T, D>(value);
      AddKeyToValue<D, T>(key, v);
    }

    private static void AddKeyToValue<D, T>(Key k, D v) where D : ICustomOptionDAO<T>, T, new()
    {
      AddKeyValue(k.Id, v.Id);
    }

    internal static async void SaveNewValues<T, D, K>(IKey<K> key, ICollection<T> values, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      Key k = await KeyFunctions.CreateAndGetKey(key, options);
      foreach (T value in values)
      {
        SaveNewValue<T, D, K>(k, value);
      }
    }

    internal static async Task<ICollection<T>> GetValues<T, D, K>(IKey<K> key) where D : ICustomOptionDAO<T>, T, new()
    {
      Key k = await KeyFunctions.GetKey(key);
      if (k == null)
        return default;
      ICollection<int> valueIds = await GetValueIds(k.Id);
      ICollection<T> values = new List<T>();
      foreach(int id in valueIds)
      {
        values.Add(await GetValueByIndex<D>(id));
      }
      return values;
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


    private static void AddKeyToValue(Key k, Value v)
    {
      AddKeyValue(k.Id, v.Id);
    }

    private static void AddKeyValue(int keyId, int valueId)
    {
      KeyValue keyValue = new KeyValue() { KeyId = keyId, ValueId = valueId };
      _ = _db.InsertAsync(keyValue);
    }
  }
}
