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
      foreach(IKey<K> k in unfoundKeys.ToList())
      {
        T value = GetFromMemory(k);
        if (!ValueIsSet(value))
          continue;
        values.Add(value);
        unfoundKeys.Remove(k);
      }
      values.AddRange(await GetFromPersistent(unfoundKeys)); //TODO: save to memory
      //foreach (K k in key.KeyValue)
      //{
      //  IKey<K> singleKey = new Key<K>(key.KeyIdentifier, k, key.ObjectType);
      //  T singleValue = GetFromMemory(singleKey);
      //  singleValue = await GetFromPersistentAndSave(singleKey, singleValue);
      //  UpdateExpiration(singleKey, singleValue);
      //  if (ValueIsSet(singleValue))
      //    values.Add(singleValue);
      //  else
      //    unfoundKeys.Add(k);
      //}
      //if (unfoundKeys.Count == 0)
      //  return values;
      //Key<IEnumerable<K>> keys = new Key<IEnumerable<K>>(key.KeyIdentifier, unfoundKeys, key.ObjectType);
      //if (!offline)
      //{
      //  ICollection<T> serviceValues = await GetFromServiceAndSave(keys, null);
      //  if (serviceValues == null)
      //    return new List<T>();
      //  values.AddRange(serviceValues);
      //}
      //else
      //{
      //  return new List<T>();
      //}
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
      throw new NotImplementedException();
      //return await PersistentManager.GetCollection<T, K>(keys);
    }
    protected override async void  SaveToPersistent(IKey<IEnumerable<K>> key, ICollection<T> value)
    {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      foreach (T singleValue in value)
      {
        IKey<K> singleKey = LoadKeyFromCurrentValue(singleValue);
        await SaveToPersistent(singleKey, singleValue);
      }
      Debug.Write("[SaveToPersistent] " + stopwatch.Elapsed.ToString());
      stopwatch.Stop();
    }

    protected virtual async Task SaveToPersistent(IKey<K> singleKey, T singleValue)
    {
      await PersistentManager.Save(singleKey, singleValue, Options);
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
