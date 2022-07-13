using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
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
      ICollection<K> unfoundKeys = new List<K>();
      foreach (K k in key.KeyValue)
      {
        IKey<K> singleKey = new Key<K>(key.KeyIdentifier, k, key.ObjectType);
        T singleValue = GetFromMemory(singleKey);
        singleValue = await GetFromPersistentAndSave(singleKey, singleValue);
        UpdateExpiration(singleKey, singleValue);
        if (ValueIsSet(singleValue))
          values.Add(singleValue);
        else
          unfoundKeys.Add(k);
      }
      if (unfoundKeys.Count == 0)
        return values;
      Key<IEnumerable<K>> keys = new Key<IEnumerable<K>>(key.KeyIdentifier, unfoundKeys, key.ObjectType);
      if (!offline)
      {
        ICollection<T> serviceValues = await GetFromServiceAndSave(keys, null);
        values.AddRange(serviceValues);
      }
      else
      {
        return new List<T>();
      }
      return values;
    }
    protected async Task<T> GetFromPersistentAndSave(IKey<K> singleKey, T singleValue)
    {
      if (ValueIsSet(singleValue))
        return singleValue;
      singleValue = await GetFromPersistent(singleKey);
      if (ValueIsSet(singleValue))
        SaveToMemory(singleKey, singleValue);
      return singleValue;
    }
    protected T GetFromMemory(IKey<K> singleKey)
    {
      return MemoryManager.Get<T, K>(singleKey);
    }
    protected virtual async Task<T> GetFromPersistent(IKey<K> singleKey)
    {
      return await PersistentManager.Get<T, K>(singleKey);
    }
    protected override void SaveToPersistent(IKey<IEnumerable<K>> key, ICollection<T> value)
    {
      foreach (T singleValue in value)
      {
        IKey<K> singleKey = LoadKeyFromCurrentValue(singleValue);
        SaveToPersistent(singleKey, singleValue);
      }
    }

    protected virtual void SaveToPersistent(IKey<K> singleKey, T singleValue)
    {
      PersistentManager.Save(singleKey, singleValue, Options);
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
