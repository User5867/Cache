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
    internal static async Task<ICollection<T>> GetValues<T, D, K>(IEnumerable<IKey<K>> keys) where D : ICustomOptionDAO<T>, T, new()
    {
      var mapping = await _db.GetMappingAsync<D>();
      List<int> hashcodes = new List<int>();
      List<string> blobs = new List<string>();
      foreach(IKey<K> key in keys)
      {
        hashcodes.Add(KeyFunctions.GetHashcode(key));
        blobs.Add(KeyFunctions.GetKeyBlob(key));
      } 
      IEnumerable<D> ds = await _db.QueryAsync<D>(string.Format(_getValueByKeySelect, mapping.TableName), hashcodes, blobs);
      return ds.Cast<T>().ToList();
    }
    private const string _getValueByKeySelect = "select * from {0} where Id = (select ValueId from KeyValue where KeyId = (select Id From Key where Deleted = false AND Hashcode = ? AND ObjectKeyBlob = '?' ));";
    internal static async Task<D> GetValue<T, D, K>(IKey<K> key) where D : ICustomOptionDAO<T>, T, new()
    {
      var mapping = await _db.GetMappingAsync<D>();
      int hashcode = KeyFunctions.GetHashcode(key);
      string s = KeyFunctions.GetKeyBlob(key);
      return await _db.FindWithQueryAsync<D>(string.Format(_getValueByKeySelect, mapping.TableName),  hashcode, s);
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
    private static async Task SaveValue<T, K>(IKey<K> key, T value, IOptions options)
    {
      await _db.RunInTransactionAsync(t =>
      {
        Key k = KeyFunctions.CreateAndGetKey(t, key, options);
        Value v = CreateAndGetValue(t, value);
        AddKeyToValue(t, k, v);
      });
    }
    internal static async Task TrySaveValue<T, K>(IKey<K> key, T value, IOptions options)
    {
      try
      {
        await SaveValue(key, value, options);
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
      ICollection<T> values = new List<T>();
      foreach (Value value in k.Values)
      {
        values.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value.ObjectBlob));
      }
      return values;
    }
    private static async Task SaveValues<K, T>(IKey<K> key, ICollection<T> values, IOptions options)
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
    internal static async Task TrySaveValues<K, T>(IKey<K> key, ICollection<T> values, IOptions options)
    {
      try
      {
        await SaveValues(key, values, options);
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Fail(e.Message);
      }
      
    }
    private static async Task SaveValue<T, D, K>(IKey<K> key, T value, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      await _db.RunInTransactionAsync(t =>
      {
        Key k = KeyFunctions.CreateAndGetKey(t, key, options);
        SaveValue<T, D, K>(t, k, value);
      });
    }
    internal static async Task TrySaveValue<T, D, K>(IKey<K> key, T value, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      try
      {
        await SaveValue<T, D, K>(key, value, options);
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Fail(e.Message);
      }
    }

    private static async Task SaveValue<T, D, K>(SQLiteConnection transaction, Key key, T value) where D : ICustomOptionDAO<T>, T, new()
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
    private static async Task SaveValues<T, D, K>(IKey<K> key, ICollection<T> values, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      await _db.RunInTransactionAsync(t =>
      {
        Key k = KeyFunctions.CreateAndGetKey(t, key, options);
        foreach (T value in values)
        {
          SaveValue<T, D, K>(t, k, value);
        }
      });
    }
    internal static async Task TrySaveValues<T, D, K>(IKey<K> key, ICollection<T> values, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      try
      {
        await SaveValues<T, D, K>(key, values, options);
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
          return null;
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
