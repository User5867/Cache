using CacheLibary.DAOs;
using CacheLibary.Interfaces;
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
      return await TryGetValueByIndex<D>(valueId.Value);
    }

    private static async Task<int?> GetValueId(int keyId)
    {
      return (await _db.Table<KeyValue>().Where(kv => kv.KeyId == keyId).FirstOrDefaultAsync())?.ValueId;
    }

    private static async Task<IEnumerable<int>> GetValueIds(int keyId)
    {
      return (await _db.Table<KeyValue>().Where(kv => kv.KeyId == keyId).ToListAsync()).Select(kv => kv.ValueId).ToList();
    }

    internal static async Task<IEnumerable<int>> GetValueIds(IEnumerable<int> keyIds)
    {
      return (await _db.Table<KeyValue>().Where(kv => keyIds.Contains(kv.KeyId)).ToListAsync()).Select(kv => kv.ValueId).Distinct();
    }

    private static async Task<T> TryGetValueByIndex<T>(int id) where T : new()
    {
      try
      {
        return await GetValueByIndex<T>(id);
      }
      catch
      {
        return default;
      }
    }
    private static async Task<T> GetValueByIndex<T>(int id) where T : new()
    {
      return await _db.GetWithChildrenAsync<T>(id);
    }
    private static async Task SaveNewValue<T, K>(IKey<K> key, T value, IOptions options)
    {
      await _db.RunInTransactionAsync(t =>
      {
        Key k = KeyFunctions.CreateAndGetKey(t, key, options);
        Value v = CreateAndGetValue(t, value);
        AddKeyToValue(t, k, v);
      });
    }
    internal static async void TrySaveNewValue<T, K>(IKey<K> key, T value, IOptions options)
    {
      try
      {
        await SaveNewValue(key, value, options);
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Fail(e.Message);
      }
    }

    private static Value CreateAndGetValue<T>(SQLiteConnection transaction, T value)
    {
      Value v = new Value() { Object = value };
      transaction.InsertWithChildren(v);
      return v;
    }

    private static D CreateAndGetValue<T, D>(SQLiteConnection transaction, T value) where D : ICustomOptionDAO<T>, T, new()
    {
      D v = new D().CreateInstance<D>(value);
      v.Id = HashFunctions.GetFirstFreeIndex<D>(v.Hashcode);
      transaction.InsertWithChildren(v);
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
    private static async Task SaveNewValues<K, T>(IKey<K> key, ICollection<T> values, IOptions options)
    {
      await _db.RunInTransactionAsync(t =>
      {
        Key k = KeyFunctions.CreateAndGetKey(t, key, options);
        ICollection<Value> v = CreateAndGetValues(t, values);
        foreach (Value value in v)
        {
          AddKeyToValue(t, k, value);
        }
      });
    }
    internal static async void TrySaveNewValues<K, T>(IKey<K> key, ICollection<T> values, IOptions options)
    {
      try
      {
        await SaveNewValues(key, values, options);
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Fail(e.Message);
      }
      
    }
    private static async Task SaveNewValue<T, D, K>(IKey<K> key, T value, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      await _db.RunInTransactionAsync(t =>
      {
        Key k = KeyFunctions.CreateAndGetKey(t, key, options);
        SaveNewValue<T, D, K>(t, k, value);
      });
    }
    internal static async void TrySaveNewValue<T, D, K>(IKey<K> key, T value, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      try
      {
        await SaveNewValue<T, D, K>(key, value, options);
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Fail(e.Message);
      }
    }

    private static async void SaveNewValue<T, D, K>(SQLiteConnection transaction, Key key, T value) where D : ICustomOptionDAO<T>, T, new()
    {
      D v = await HashFunctions.GetByHashcode<D, T>(new D().CreateInstance<D>(value).Hashcode, value);
      if (v == null)
        v = CreateAndGetValue<T, D>(transaction, value);
      AddKeyToValue<D, T>(transaction, key, v);
    }

    private static void AddKeyToValue<D, T>(SQLiteConnection transaction, Key k, D v) where D : ICustomOptionDAO<T>, T, new()
    {
      AddKeyValue(transaction, k.Id, v.Id);
    }
    private static async Task SaveNewValues<T, D, K>(IKey<K> key, ICollection<T> values, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      await _db.RunInTransactionAsync(t =>
      {
        Key k = KeyFunctions.CreateAndGetKey(t, key, options);
        foreach (T value in values)
        {
          SaveNewValue<T, D, K>(t, k, value);
        }
      });
    }
    internal static async void TrySaveNewValues<T, D, K>(IKey<K> key, ICollection<T> values, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      try
      {
        await SaveNewValues<T, D, K>(key, values, options);
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Fail(e.Message);
      }
    }

    internal static async Task<ICollection<T>> GetValues<T, D, K>(IKey<K> key) where D : ICustomOptionDAO<T>, T, new()
    {
      Key k = await KeyFunctions.GetKey(key);
      if (k == null)
        return default;
      IEnumerable<int> valueIds = await GetValueIds(k.Id);
      ICollection<T> values = new List<T>();
      foreach (int id in valueIds)
      {
        D v = await TryGetValueByIndex<D>(id);
        if (v == null)
          return new List<T>();
        values.Add(v);
      }
      return values;
    }

    private static ICollection<Value> CreateAndGetValues<T>(SQLiteConnection transaction, ICollection<T> values)
    {
      ICollection<Value> retVal = new List<Value>();
      foreach (T value in values)
      {
        retVal.Add(CreateAndGetValue(transaction, value));
      }
      return retVal;
    }


    private static void AddKeyToValue(SQLiteConnection transaction, Key k, Value v)
    {
      AddKeyValue(transaction, k.Id, v.Id);
    }

    private static void AddKeyValue(SQLiteConnection transaction, int keyId, int valueId)
    {
      KeyValue keyValue = new KeyValue() { KeyId = keyId, ValueId = valueId };
      _ = transaction.Insert(keyValue);
    }
  }
}
