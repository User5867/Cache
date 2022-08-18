using CacheLibary.DAOs;
using CacheLibary.Interfaces;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Concurrent;
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
    private const string _getValueByValueId = "select {0}.* from {0} where UniqueId = '{1}';";
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
      System.Diagnostics.Debug.Write(keys.ToList().Count + ": " + st);
      IEnumerable<D> ds = await _db.QueryAsync<D>(st);
      System.Diagnostics.Debug.Write(1234567890);
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
      await _db.RunInTransactionAsync(t =>
      {
        Key k = KeyFunctions.CreateAndGetKey(t, key, options).Result;
        if (k == null)
          return;
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
        System.Diagnostics.Debug.Write(e.Message);
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
      _ = transaction.Insert(value);
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
      await _db.RunInTransactionAsync(t =>
      {
        IEnumerable<IKey<K>> keys = keyValues.AsParallel().Select(kv => kv.Key);
        ICollection<T> values = keyValues.AsParallel().Select(kv => kv.Value).ToList();
        IEnumerable<Key> ks = KeyFunctions.CreateAndGetKeys(t, keys, options).Result;
        System.Diagnostics.Debug.Write(8);
        ICollection<D> daos = GetDaoValues<T, D>(values);
        System.Diagnostics.Debug.Write(7);
        List<D> savedValues = GetSavedValues<T, D>(daos).Result;
        System.Diagnostics.Debug.Write(6);
        IEnumerable<string> savedValueUniqueIds = savedValues.AsParallel().Select(s => s.UniqueId);
        System.Diagnostics.Debug.Write(5);
        SaveMissingDaos<T, D>(t, daos, savedValues, savedValueUniqueIds);
        System.Diagnostics.Debug.Write(4);
        SaveKeyValues(t, ks, savedValues, keyValues);
        System.Diagnostics.Debug.Write(3);
      });
    }

    internal static async Task<IEnumerable<T>> GetValues<T, K>(IEnumerable<IKey<K>> keys)
    {
      string b = "'" + string.Join("', '", keys.Select(s => s.KeyBlob)) + "'";
      string st = string.Format(_getValuesByKeysSelect, nameof(Value), b);
      IEnumerable<Value> ds = await _db.QueryAsync<Value>(st);
      ICollection<T> values = new List<T>();
      _ = Parallel.ForEach(ds, d => values.Add(JsonConvert.DeserializeObject<T>(d.ObjectBlob)));
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
      _ = Parallel.ForEach(values, v => daos.Add(new Value { ObjectBlob = JsonConvert.SerializeObject(v) }));
      return daos;
    }

    private static void SaveKeyValues<T, D, K>(SQLiteConnection transaction, IEnumerable<Key> ks, IEnumerable<D> savedValues, IEnumerable<KeyValuePair<IKey<K>, T>> keyValues) where D : ICustomOptionDAO<T>, T, new()
    {
      ConcurrentBag<KeyValue> kvs = new ConcurrentBag<KeyValue>();
      ILookup<string, Key> keys = ks.ToLookup(k => k.ObjectKeyBlob);
      ILookup<string, D> values = savedValues.ToLookup(v => v.UniqueId);
      D d = new D();
      _ = Parallel.ForEach(keyValues, kvp => 
      {
        if (!keys.Contains(kvp.Key.KeyBlob))
          return;
        int keyId = keys[kvp.Key.KeyBlob].First().Id;
        int valueId = values[d.CreateInstance<D>(kvp.Value).UniqueId].First().Id;
        kvs.Add(new KeyValue { KeyId = keyId, ValueId = valueId });
      });
      IEnumerable<KeyValue> existingKeyValues = GetExistingKeyValues(transaction, kvs);
      _ = transaction.InsertAll(kvs.Where(kv => existingKeyValues.Count(e => e.KeyId == kv.KeyId && e.ValueId == kv.ValueId) == 0));
    }
    private static string GetKeyValues = "select * from KeyValue where KeyId in ({0}) and ValueId in ({1})";
    private static void SaveKeyValues<T, K>(SQLiteConnection transaction, IEnumerable<Key> ks, IEnumerable<Value> savedValues, IEnumerable<KeyValuePair<IKey<K>, T>> keyValues)
    {
      ConcurrentBag<KeyValue> kvs = new ConcurrentBag<KeyValue>();
      ILookup<string, Key> keys = ks.ToLookup(k => k.ObjectKeyBlob);
      ILookup<string, Value> values = savedValues.ToLookup(v => v.ObjectBlob);
      _ = Parallel.ForEach(keyValues, kvp =>
      {
        int keyId = keys[kvp.Key.KeyBlob].First().Id;
        int valueId = values[JsonConvert.SerializeObject(kvp.Value)].First().Id;
        kvs.Add(new KeyValue { KeyId = keyId, ValueId = valueId });
      });
      IEnumerable<KeyValue> existingKeyValues = GetExistingKeyValues(transaction, kvs);
      _ = transaction.InsertAll(kvs.Except(existingKeyValues));
    }

    private static IEnumerable<KeyValue> GetExistingKeyValues(SQLiteConnection transaction, ConcurrentBag<KeyValue> kvs)
    {
      IEnumerable<int> keyIds = kvs.Select(kv => kv.KeyId);
      IEnumerable<int> valueIds = kvs.Select(kv => kv.ValueId);
      string keyString =  string.Join(", ", keyIds);
      string valueString = string.Join(", ", valueIds);
      string query = string.Format(GetKeyValues, keyString, valueString);
      IEnumerable<KeyValue> keyValue = transaction.Query<KeyValue>(query);
      ConcurrentBag<KeyValue> existingKeyValues = new ConcurrentBag<KeyValue>();
      _ = Parallel.ForEach(keyValue, kv =>
      {
        if (kvs.Contains(kv))
          existingKeyValues.Add(kv);
      }
      );
      if(keyValue.Count() > 0 || existingKeyValues.Count > 0)
      {

      }

      return existingKeyValues;
    }

    private static async Task SaveValues<T, K>(IKey<K> key, ICollection<T> values, IOptions options)
    {
      await _db.RunInTransactionAsync(t =>
      {
        Key k = KeyFunctions.CreateAndGetKey(t, key, options).Result;
        if (k == null)
          return;
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
        if (k == null)
          return;
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
        TableMapping mapping = await _db.GetMappingAsync<D>();
        string query = string.Format(_getValueByValueId, mapping.TableName, d.UniqueId);
        return await _db.FindWithQueryAsync<D>(query);
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
        if (k == null)
          return;
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
      ILookup<string, string> sv = savedValueUniqueIds.ToLookup(s => s);
      ICollection<D> missingDaos = daos.AsParallel().Where(da => !sv.Contains(da.UniqueId)).ToList();
      _ = t.InsertAll(missingDaos);
      if(missingDaos.Count > 0)
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
      _ = Parallel.ForEach(savedValues, s => keyValues.Add(new KeyValue { ValueId = s.Id, KeyId = k.Id }));
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
      ConcurrentBag<D> daos = new ConcurrentBag<D>();
      D d = new D();
      if(false && values.Count > 10000)
      {
        int c = values.Count / 10000 + 1;
        List<List<T>> vs = values.Select((v, index) => new { Index = index, Value = v }).GroupBy(x => x.Index % c).Select(x => x.Select(v => v.Value).ToList()).ToList();
        foreach(ICollection<T> co in vs)
        {
          _ = Parallel.ForEach(co, v => daos.Add(d.CreateInstance<D>(v)));
        }
        return daos.ToList();
      }
      _ = Parallel.ForEach(values, v => daos.Add(d.CreateInstance<D>(v)));
      return daos.ToList();
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
    private const string KeyValueNotExist = "select not exists(select 1 from KeyValue where KeyId = {0} and ValueId = {1})";
    private static void AddKeyValue(SQLiteConnection transaction, int keyId, int valueId)
    {
      KeyValue keyValue = new KeyValue() { KeyId = keyId, ValueId = valueId };
      string query = string.Format(KeyValueNotExist, keyId, valueId);
      bool keyValueNotExist = transaction.ExecuteScalar<bool>(query);
      if (keyValueNotExist)
        _ = transaction.Insert(keyValue);
    }
  }
}
