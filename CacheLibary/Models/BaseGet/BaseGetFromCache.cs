using CacheLibary.Interfaces;
using CacheLibary.Interfaces.CacheManager;
using CacheLibary.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models
{
  internal abstract class BaseGetFromCache<T, K> : IBaseGetFromCache<T, K>
  {
    protected IOptions Options { get; private set; }
    protected IMemoryCacheManager MemoryManager { get; } = MemoryCacheManager.Instance;
    protected IPersistentCacheManager PersistentManager { get; } = PersistentCacheManager.Instance;
    internal BaseGetFromCache(IOptions options)
    {
      Options = options;
    }

    protected virtual async Task<T> Get(IKey<K> key, bool offline = false)
    {
      T value = GetFromMemory(key);
      value = await GetFromPersistentAndSave(key, value);
      UpdateExpiration(key, value);
      if(!offline)
        value = await GetFromServiceAndSave(key, value);
      return value;
    }

    protected virtual bool ValueIsSet(T value)
    {
      return !IsNull(value);
    }
    protected bool IsNull(object t)
    {
      return t == null;
    }
    protected virtual T GetFromMemory(IKey<K> key)
    {
      return MemoryManager.Get<T, K>(key);
    }
    protected virtual void SaveToPersistent(IKey<K> key, T value)
    {
      PersistentManager.Save(key, value, Options);
    }
    protected virtual void SaveToMemory(IKey<K> key, T value)
    {
      MemoryManager.Save(key, value, Options);
    }
    protected virtual async Task<T> GetFromPersistentAndSave(IKey<K> key, T value)
    {
      if (ValueIsSet(value))
        return value;
      value = await GetFromPersistent(key);
      if (ValueIsSet(value))
        SaveToMemory(key, value);
      return value;
    }

    protected virtual void UpdateExpiration(IKey<K> key, T value)
    {
      if (ValueIsSet(value))
        PersistentManager.UpdateExpiration(key);
    }

    protected async Task<T> GetFromServiceAndSave(IKey<K> key, T value)
    {
      if (ValueIsSet(value))
        return value;
      try
      {
        value = await GetFromService(key);
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Write("[BaseGetFromCache.GetFromService]" + e.Message);
        return value; 
      }
      SaveToCache(key, value);
      return value;
    }

    private void SaveToCache(IKey<K> key, T value)
    {
      if (!ValueIsSet(value))
        return;
      SaveToMemory(key, value);
      SaveToPersistent(key, value);
    }

    protected async virtual Task<T> GetFromPersistent(IKey<K> key)
    {
      return await PersistentManager.Get<T, K>(key);
    }
    protected abstract Task<T> GetFromService(IKey<K> key);
    public abstract Task<T> Get(K key);

    public IBaseGetFromCache<T, K1> GetBaseGetFromCache<K1>()
    {
      if (typeof(K1) == typeof(K))
        return (IBaseGetFromCache<T, K1>)this;
      throw new Exception();
    }
  }
}
