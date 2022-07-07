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
    protected T Value { get; set; }
    protected IOptions Options { get; private set; }
    protected IKey<K> Key { get; set; }
    protected IMemoryCacheManager MemoryManager { get; } = MemoryCacheManager.Instance;
    protected IPersistentCacheManager PersistentManager { get; } = PersistentCacheManager.Instance;
    internal BaseGetFromCache(IOptions options)
    {
      Options = options;
    }

    protected virtual async Task<T> Get(IKey<K> key, bool offline = false)
    {
      ClearPropertys();
      Key = key;
      GetFromMemory();
      await GetFromPersistentAndSave();
      UpdateExpiration();
      if(!offline)
        await GetFromServiceAndSave();
      return Value;
    }

    protected virtual void ClearPropertys()
    {
      Value = default;
      Key = null;
    }
    protected virtual bool ValueIsSet()
    {
      return !IsNull(Value);
    }
    protected bool IsNull(object t)
    {
      return t == null;
    }
    protected virtual void GetFromMemory()
    {
      Value = MemoryManager.Get<T, K>(Key);
    }
    protected virtual void SaveToPersistent()
    {
      PersistentManager.Save(Key, Value, Options);
    }
    protected virtual void SaveToMemory()
    {
      MemoryManager.Save(Key, Value, Options);
    }
    protected virtual async Task GetFromPersistentAndSave()
    {
      if (ValueIsSet())
        return;
      await GetFromPersistent();
      if (ValueIsSet())
        SaveToMemory();
    }

    protected virtual void UpdateExpiration()
    {
      if (ValueIsSet())
        PersistentManager.UpdateExpiration(Key);
    }

    protected async Task GetFromServiceAndSave()
    {
      if (ValueIsSet())
        return;
      try
      {
        await GetFromService();
      }
      catch (Exception e)
      {
        System.Diagnostics.Debug.Write("[BaseGetFromCache.GetFromService]" + e.Message);
        return; 
      }
      
      if (ValueIsSet())
        SaveToCache();
    }

    private void SaveToCache()
    {
      SaveToMemory();
      SaveToPersistent();
    }

    protected async virtual Task GetFromPersistent()
    {
      Value = await PersistentManager.Get<T, K>(Key);
    }
    protected abstract Task GetFromService();
    public abstract Task<T> Get(K key);

    public IBaseGetFromCache<T, K1> GetBaseGetFromCache<K1>()
    {
      if (typeof(K1) == typeof(K))
        return (IBaseGetFromCache<T, K1>)this;
      throw new Exception();
    }
  }
}
