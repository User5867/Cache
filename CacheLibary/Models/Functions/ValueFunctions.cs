using CacheLibary.DAOs;
using CacheLibary.Interfaces;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models.Functions
{
  internal class ValueFunctions
  {
    private const string _getValuesByKeysSelect = "select {0}.* from {0} join (select ValueId from KeyValue join ( select Id from Key where Deleted = false and ObjectKeyBlob in ( {1} ) ) on Id = KeyId ) on Id = ValueId;";
    private const string _getValueByKeySelect = "select {0}.* from {0} where Id = (select ValueId from KeyValue join ( select Id from Key where Deleted = false and ObjectKeyBlob = ?) where KeyId = Id );";
    private const string _getValuesByKeySelect = "select {0}.* from {0} join (select ValueId from KeyValue join ( select Id from Key where Deleted = false and ObjectKeyBlob = ?) where KeyId = Id ) on Id = ValueId;";
    private const string _getValuesByValueIds = "select {0}.* from {0} where UniqueId in ( {1} );";
    private static SQLiteAsyncConnection _db = PersistentCacheManager.Instance.GetDatabase();

    internal static async Task<T> GetValue<T, K>(IKey<K> key)
    {
      string s = key.KeyBlob;
      Value o = await _db.FindWithQueryAsync<Value>(string.Format(_getValueByKeySelect, nameof(Value)), s);
      if (o != null && o.ObjectBlob != null)
        return JsonConvert.DeserializeObject<T>(o.ObjectBlob);
      return default;
    }
    internal static async Task<ICollection<T>> GetValues<T, D, K>(IEnumerable<IKey<K>> keys) where D : ICustomOptionDAO<T>, T, new()
    {
      TableMapping mapping = await _db.GetMappingAsync<D>();
      string b = "'" + string.Join("', '", keys.Select(s => s.KeyBlob)) + "'";
      string st = string.Format(_getValuesByKeysSelect, mapping.TableName, b);
      IEnumerable<D> ds = await _db.QueryAsync<D>(st);
      return ds.Cast<T>().ToList();
    }
    
    internal static async Task<D> GetValue<T, D, K>(IKey<K> key) where D : ICustomOptionDAO<T>, T, new()
    {
      TableMapping mapping = await _db.GetMappingAsync<D>();
      string s = key.KeyBlob;
      return await _db.FindWithQueryAsync<D>(string.Format(_getValueByKeySelect, mapping.TableName), s);
    }
    private static async Task SaveValue<T, K>(IKey<K> key, T value, IOptions options)
    {
      await _db.RunInTransactionAsync(async t =>
      {
        Key k = await KeyFunctions.CreateAndGetKey(t, key, options);
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
      Value v = new Value() { ObjectBlob = JsonConvert.SerializeObject(value) };
      transaction.Insert(v);
      return v;
    }

    private static D InsertAndGetValue<D>(SQLiteConnection transaction, D value)
    {
      transaction.Insert(value);
      return value;
    }
    internal static async Task<ICollection<T>> GetValues<T, K>(IKey<K> key)
    {
      string s = key.KeyBlob;
      Value value = await _db.FindWithQueryAsync<Value>(string.Format(_getValueByKeySelect, nameof(Value)), s);
      if (value == null)
        return default;
      return JsonConvert.DeserializeObject<ICollection<T>>(value.ObjectBlob);
    }

    internal static async Task TrySaveValues<T, D, K>(IEnumerable<KeyValuePair<IKey<K>, T>> keyValues, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      try
      {
        await SaveValues<T, D, K>(keyValues, options);
      }
      catch(Exception e)
      {
        System.Diagnostics.Debug.Write(e.Message);
      }
    }

    private static async Task SaveValues<T, D, K>(IEnumerable<KeyValuePair<IKey<K>, T>> keyValues, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      //TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
      await _db.RunInTransactionAsync(t =>
      {
        IEnumerable<IKey<K>> keys = keyValues.Select(kv => kv.Key);
        ICollection<T> values = keyValues.Select(kv => kv.Value).ToList();
        IEnumerable<Key> ks = KeyFunctions.CreateAndGetKeys(t, keys, options).Result;

        ICollection<D> daos = GetDaoValues<T, D>(values);
        List<D> savedValues = GetSavedValues<T, D>(daos).Result;
        IEnumerable<string> savedValueUniqueIds = savedValues.Select(s => s.UniqueId);
        SaveMissingDaos<T, D>(t, daos, savedValues, savedValueUniqueIds);
        SaveKeyValues(t, ks, savedValues, keyValues);
        //tcs.SetResult(true);
      });
      //bool b = await tcs.Task;
    }

    internal static async Task<IEnumerable<T>> GetValues<T, K>(IEnumerable<IKey<K>> keys)
    {
      string b = "'" + string.Join("', '", keys.Select(s => s.KeyBlob)) + "'";
      string st = string.Format(_getValuesByKeysSelect, nameof(Value), b);
      IEnumerable<Value> ds = await _db.QueryAsync<Value>(st);
      ICollection<T> values = new List<T>();
      foreach(Value value in ds)
      {
        values.Add(JsonConvert.DeserializeObject<T>(value.ObjectBlob));
      }
      return values;
    }

    internal static async Task TrySaveValues<T, K>(IEnumerable<KeyValuePair<IKey<K>, T>> keyValues, IOptions options)
    {
      try
      {
        await SaveValues(keyValues, options);
      }
      catch(Exception e)
      {
        System.Diagnostics.Debug.Write(e.Message);
      }
    }

    private static async Task SaveValues<T, K>(IEnumerable<KeyValuePair<IKey<K>, T>> keyValues, IOptions options)
    {
      await _db.RunInTransactionAsync(t =>
      {
        IEnumerable<IKey<K>> keys = keyValues.Select(kv => kv.Key);
        ICollection<T> values = keyValues.Select(kv => kv.Value).ToList();
        IEnumerable<Key> ks = KeyFunctions.CreateAndGetKeys(t, keys, options).Result;

        ICollection<Value> daos = GetValues(values);
        _ = t.InsertAll(daos);
        SaveKeyValues(t, ks, daos, keyValues);
      });
    }

    private static ICollection<Value> GetValues<T>(ICollection<T> values)
    {
      ICollection<Value> daos = new List<Value>();
      foreach(T value in values)
      {
        daos.Add(new Value { ObjectBlob = JsonConvert.SerializeObject(value) });
      }
      return daos;
    }

    private static void SaveKeyValues<T, D, K>(SQLiteConnection transaction, IEnumerable<Key> ks, IEnumerable<D> savedValues, IEnumerable<KeyValuePair<IKey<K>, T>> keyValues) where D : ICustomOptionDAO<T>, T, new()
    {
      ICollection<KeyValue> kvs = new List<KeyValue>();
      ILookup<string, Key> keys = ks.ToLookup(k => k.ObjectKeyBlob);
      ILookup<string, D> values = savedValues.ToLookup(v => v.UniqueId);
      D d = new D();
      foreach (KeyValuePair<IKey<K>, T> kvp in keyValues)
      {
        int keyId = keys[kvp.Key.KeyBlob].First().Id;
        int valueId = values[d.CreateInstance<D>(kvp.Value).UniqueId].First().Id;
        kvs.Add(new KeyValue { KeyId = keyId, ValueId = valueId });
      }
      _ = transaction.InsertAll(kvs);
    }
    private static void SaveKeyValues<T, K>(SQLiteConnection transaction, IEnumerable<Key> ks, IEnumerable<Value> savedValues, IEnumerable<KeyValuePair<IKey<K>, T>> keyValues)
    {
      ICollection<KeyValue> kvs = new List<KeyValue>();
      ILookup<string, Key> keys = ks.ToLookup(k => k.ObjectKeyBlob);
      ILookup<string, Value> values = savedValues.ToLookup(v => v.ObjectBlob);
      foreach (KeyValuePair<IKey<K>, T> kvp in keyValues)
      {
        int keyId = keys[kvp.Key.KeyBlob].First().Id;
        int valueId = values[JsonConvert.SerializeObject(kvp.Value)].First().Id;
        kvs.Add(new KeyValue { KeyId = keyId, ValueId = valueId });
      }
      _ = transaction.InsertAll(kvs);
    }

    private static async Task SaveValues<T, K>(IKey<K> key, ICollection<T> values, IOptions options)
    {
      await _db.RunInTransactionAsync(t =>
      {
        Key k = KeyFunctions.CreateAndGetKey(t, key, options).Result;
        Value v = CreateAndGetValue(t, values);
        AddKeyToValue(t, k, v);
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
        System.Diagnostics.Debug.Write(e.Message);
      }

    }
    private static async Task SaveValue<T, D, K>(IKey<K> key, T value, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      await _db.RunInTransactionAsync(t =>
      {
        Key k = KeyFunctions.CreateAndGetKey(t, key, options).Result;
        _ = SaveValue<T, D, K>(t, k, value);
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
        System.Diagnostics.Debug.Write(e.Message);
      }
    }

    private static async Task SaveValue<T, D, K>(SQLiteConnection transaction, Key key, T value) where D : ICustomOptionDAO<T>, T, new()
    {
      D d = new D().CreateInstance<D>(value);
      D v = await TryGetValue<T, D>(d);
      if (v == null)
        v = InsertAndGetValue(transaction, d);
      AddKeyToValue<D, T>(transaction, key, v);
    }

    private static async Task<D> TryGetValue<T, D>(D d) where D : ICustomOptionDAO<T>, T, new()
    {
      try
      {
        return await _db.GetAsync<D>(d.Id);
      }
      catch(Exception e)
      {
        System.Diagnostics.Debug.Write(e.Message);
        return default;
      }
      
    }

    private static void AddKeyToValue<D, T>(SQLiteConnection transaction, Key k, D v) where D : ICustomOptionDAO<T>, T, new()
    {
      AddKeyValue(transaction, k.Id, v.Id);
    }
    private static async Task SaveValues<T, D, K>(IKey<K> key, ICollection<T> values, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      await _db.RunInTransactionAsync(t =>
      {
        Key k = KeyFunctions.CreateAndGetKey(t, key, options).Result;
        _ = SaveValues<T, D, K>(t, k, values);
      });
    }

    private static async Task SaveValues<T, D, K>(SQLiteConnection t, Key k, ICollection<T> values) where D : ICustomOptionDAO<T>, T, new()
    {
      ICollection<D> daos = GetDaoValues<T, D>(values);
      List<D> savedValues = await GetSavedValues<T, D>(daos);
      IEnumerable<string> savedValueUniqueIds = savedValues.Select(s => s.UniqueId);
      SaveMissingDaos<T, D>(t, daos, savedValues, savedValueUniqueIds);
      SaveKeyValues<T, D>(t, k, savedValues);
    }

    private static void SaveMissingDaos<T, D>(SQLiteConnection t, ICollection<D> daos, List<D> savedValues, IEnumerable<string> savedValueUniqueIds) where D : ICustomOptionDAO<T>, T, new()
    {
      ICollection<D> missingDaos = daos.Where(da => !savedValueUniqueIds.Contains(da.UniqueId)).ToList();
      _ = t.InsertAll(missingDaos);
      savedValues.AddRange(missingDaos);
    }

    private static async Task<List<D>> GetSavedValues<T, D>(ICollection<D> daos) where D : ICustomOptionDAO<T>, T, new()
    {
      TableMapping mapping = await _db.GetMappingAsync<D>();
      string query = GetQuery<T, D>(daos, mapping);
      List<D> savedValues = await _db.QueryAsync<D>(query);
      return savedValues;
    }

    private static void SaveKeyValues<T, D>(SQLiteConnection t, Key k, IEnumerable<D> savedValues) where D : ICustomOptionDAO<T>, T, new()
    {
      ICollection<KeyValue> keyValues = new List<KeyValue>();
      foreach (D value in savedValues)
      {
        keyValues.Add(new KeyValue { ValueId = value.Id, KeyId = k.Id });
      }
      int i = t.InsertAll(keyValues);
    }

    private static string GetQuery<T, D>(ICollection<D> daos, TableMapping mapping) where D : ICustomOptionDAO<T>, T, new()
    {
      IEnumerable<string> valueIds = daos.Select(da => da.UniqueId);
      string ids = "'" + string.Join("', '", valueIds) + "'";
      string query = string.Format(_getValuesByValueIds, mapping.TableName, ids);
      return query;
    }

    private static ICollection<D> GetDaoValues<T, D>(ICollection<T> values) where D : ICustomOptionDAO<T>, T, new()
    {
      ICollection<D> daos = new List<D>();
      D d = new D();
      foreach (T value in values)
      {
        daos.Add(d.CreateInstance<D>(value));
      }

      return daos;
    }

    internal static async Task TrySaveValues<T, D, K>(IKey<K> key, ICollection<T> values, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      try
      {
        await SaveValues<T, D, K>(key, values, options);
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Write(e.Message);
      }
    }

    internal static async Task<ICollection<T>> GetValues<T, D, K>(IKey<K> key) where D : ICustomOptionDAO<T>, T, new()
    {
      TableMapping mapping = await _db.GetMappingAsync<D>();
      string s = key.KeyBlob;
      List<D> values = await _db.QueryAsync<D>(string.Format(_getValuesByKeySelect, mapping.TableName), s);
      ICollection<T> returnValues = values.Cast<T>().ToList();
      if (returnValues.Count > 0)
        return returnValues;
      return null;
    }

    private static ICollection<Value> InsertAndGetValues(SQLiteConnection transaction, ICollection<Value> values)
    {
      transaction.Insert(values);
      return values;
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
