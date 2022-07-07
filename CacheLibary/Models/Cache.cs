using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models
{
  internal abstract class Cache<T, D> : Cache<T> where D : new()
  {
    protected new Dictionary<int, IBaseGetFromCacheExternal<T, D>> _getFromCache = new Dictionary<int, IBaseGetFromCacheExternal<T, D>>();
    protected new Dictionary<int, IBaseGetCollectionFromCacheExternal<T, D>> _getCollectionFromCache = new Dictionary<int, IBaseGetCollectionFromCacheExternal<T, D>>();
    protected override async Task DeleteAllExpired()
    {
      await PersistentCacheManager.Instance.DeleteAllExpired<D>(typeof(T));
    }
  }
  internal abstract class Cache<T> : ICache
  {
    protected Dictionary<int, IBaseGetFromCache<T>> _getFromCache = new Dictionary<int, IBaseGetFromCache<T>>();
    protected Dictionary<int, IBaseGetCollectionFromCache<T>> _getCollectionFromCache = new Dictionary<int, IBaseGetCollectionFromCache<T>>();
    public abstract IOptions Options { get; }
    protected bool _isRunning;
    public Cache()
    {
      RunPersistentExpiration();
    }
    protected async void RunPersistentExpiration()
    {
      if (_isRunning)
        return;
      _isRunning = true;
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
