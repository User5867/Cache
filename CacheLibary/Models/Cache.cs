using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models
{
  internal abstract class Cache<T, D> : Cache<T> where D : new()
  {
    protected new Dictionary<string, IBaseGetFromCacheExternal<T, D>> _getFromCache = new Dictionary<string, IBaseGetFromCacheExternal<T, D>>();
    protected new Dictionary<string, IBaseGetCollectionFromCacheExternal<T, D>> _getCollectionFromCache = new Dictionary<string, IBaseGetCollectionFromCacheExternal<T, D>>();
    protected override async Task DeleteAllExpired()
    {
      await PersistentCacheManager.Instance.DeleteAllExpired<D>(typeof(T));
    }
  }
  internal abstract class Cache<T> : ICache
  {
    protected Dictionary<string, IBaseGetFromCache<T>> _getFromCache = new Dictionary<string, IBaseGetFromCache<T>>();
    protected Dictionary<string, IBaseGetCollectionFromCache<T>> _getCollectionFromCache = new Dictionary<string, IBaseGetCollectionFromCache<T>>();
    public abstract IOptions Options { get; }
    protected bool _isRunning;
    protected async void RunPersistentExpiration()
    {
      if (_isRunning)
        return;
      while (_isRunning)
      {
        await DeleteAllExpired();
        await Task.Delay(PersistentCacheManager.CheckInterval);
      }
    }
    protected async virtual Task DeleteAllExpired()
    {
      throw new NotImplementedException();
    }
  }
}
