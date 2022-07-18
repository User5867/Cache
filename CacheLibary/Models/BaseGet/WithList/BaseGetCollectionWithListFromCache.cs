using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models.BaseGet
{
  abstract class BaseGetCollectionWithListFromCache<T, K> : BaseGetCollectionFromCache<T, IEnumerable<K>>
  {
    internal BaseGetCollectionWithListFromCache(IOptions options) : base(options)
    {
    }
    
    protected override async Task<ICollection<T>> Get(IKey<IEnumerable<K>> key, bool offline = false)
    {
      List<T> values = new List<T>();
      ICollection<IKey<K>> unfoundKeys = new List<IKey<K>>();
      foreach (K k in key.KeyValue)
      {
        unfoundKeys.Add(new Key<K>(key.KeyIdentifier, k, key.ObjectType));
      }
      foreach (IKey<K> k in unfoundKeys.ToList())
      {
        T value = GetFromMemory(k);
        if (!ValueIsSet(value))
          continue;
        values.Add(value);
        _ = unfoundKeys.Remove(k);
      }
      if (unfoundKeys.Count == 0)
        return values;
      IEnumerable<T> persistentValues = await GetFromPersistent(unfoundKeys);
      foreach (T value in persistentValues)
      {
        IKey<K> foundKey = LoadKeyFromCurrentValue(value);
        SaveToMemory(foundKey, value);
        _ = unfoundKeys.Remove(foundKey);
      }
      values.AddRange(persistentValues);
      if (unfoundKeys.Count == 0)
        return values;
      Key<IEnumerable<K>> keys = new Key<IEnumerable<K>>(key.KeyIdentifier, unfoundKeys.Select(k => k.KeyValue), key.ObjectType);
      if (!offline)
      {
        ICollection<T> serviceValues = await GetFromServiceAndSave(keys, null);
        if (serviceValues == null)
          return new List<T>();
        values.AddRange(serviceValues);
      }
      else
      {
        return new List<T>();
      }
      return values;
    }
    //protected async Task<T> GetFromPersistentAndSave(IKey<K> singleKey, T singleValue)
    //{
    //  if (ValueIsSet(singleValue))
    //    return singleValue;
    //  singleValue = await GetFromPersistent(singleKey);
    //  if (ValueIsSet(singleValue))
    //    SaveToMemory(singleKey, singleValue);
    //  return singleValue;
    //}
    protected T GetFromMemory(IKey<K> singleKey)
    {
      return MemoryManager.Get<T, K>(singleKey);
    }
    protected virtual async Task<IEnumerable<T>> GetFromPersistent(IEnumerable<IKey<K>> keys)
    {
      return await PersistentManager.GetCollection<T, K>(keys);
    }
    protected override async void SaveToPersistent(IKey<IEnumerable<K>> key, ICollection<T> value)
    {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      ICollection<KeyValuePair<IKey<K>, T>> keyValues = new List<KeyValuePair<IKey<K>, T>>();
      foreach (T singleValue in value)
      {
        IKey<K> singleKey = LoadKeyFromCurrentValue(singleValue);
        keyValues.Add(new KeyValuePair<IKey<K>, T>(singleKey, singleValue));
      }
      await SaveToPersistent(keyValues);
      Debug.Write("[SaveToPersistent] " + stopwatch.Elapsed.ToString());
      stopwatch.Stop();
    }

    protected virtual async Task SaveToPersistent(IEnumerable<KeyValuePair<IKey<K>, T>> keyValues)
    {
      //throw new NotImplementedException();
      await PersistentManager.SaveCollection(keyValues, Options);
    }

    protected override void SaveToMemory(IKey<IEnumerable<K>> key, ICollection<T> value)
    {
      foreach (T singleValue in value)
      {
        IKey<K> singleKey = LoadKeyFromCurrentValue(singleValue);
        SaveToMemory(singleKey, singleValue);
      }
    }
    protected virtual void SaveToMemory(IKey<K> singleKey, T singleValue)
    {
      MemoryManager.Save(singleKey, singleValue, Options);
    }
    protected void UpdateExpiration(IKey<K> singleKey, T singleValue)
    {
      if (ValueIsSet(singleValue))
        PersistentManager.UpdateExpiration(singleKey);
    }
    protected bool ValueIsSet(T singleValue)
    {
      return !IsNull(singleValue);
    }
    protected abstract IKey<K> LoadKeyFromCurrentValue(T singleValue);
  }
}
