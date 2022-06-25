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
    protected async Task<T> Get(IKey<K> key)
    {
      ClearPropertys();
      Key = key;
      GetFromMemory();
      await GetFromPersistentAndSave();
      await GetFromServiceAndSave();
      return Value;
    }

    private void ClearPropertys()
    {
      Value = default;
      Key = null;
    }
    protected bool ValueIsSet()
    {
      return !IsNull(Value);
    }
    private bool IsNull(T t)
    {
      return t == null;
    }
    protected void GetFromMemory()
    {
      Value = MemoryManager.Get<T, K>(Key);
    }
    protected virtual void SaveToPersistent()
    {
      PersistentManager.Save(Key, Value, Options);
    }
    protected void SaveToMemory()
    {
      MemoryManager.Save(Key, Value, Options);
    }
    private async Task GetFromPersistentAndSave()
    {
      if (ValueIsSet())
        return;
      await GetFromPersistent();
      if (ValueIsSet())
        SaveToMemory();
    }
    private async Task GetFromServiceAndSave()
    {
      if (ValueIsSet())
        return;
      await GetFromService();
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
  }
}
