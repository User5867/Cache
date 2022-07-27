using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CacheLibary.Models
{
  internal abstract class Cache<T, D> : Cache<T> where D : ICustomOptionDAO, new()
  {
    private const string WriteToFile = "[{0}] {1}";
    private const string DeleteTime = "DeleteTest1.txt";
    private Stopwatch _stopwatchExpire = new Stopwatch();

    protected new Dictionary<int, IBaseGetFromCacheExternal<T, D>> _getFromCache = new Dictionary<int, IBaseGetFromCacheExternal<T, D>>();
    protected new Dictionary<int, IBaseGetCollectionFromCacheExternal<T, D>> _getCollectionFromCache = new Dictionary<int, IBaseGetCollectionFromCacheExternal<T, D>>();
    protected override async Task DeleteAllExpired()
    {
      _stopwatchExpire.Restart();
      await PersistentCacheManager.Instance.DeleteAllExpired<D>(typeof(T));
      _stopwatchExpire.Stop();
      StreamWriter file = new StreamWriter(DependencyService.Get<IFileHelper>().GetLocalFilePath(DeleteTime), true);
      await file.WriteLineAsync(string.Format(WriteToFile, DateTime.UtcNow, _stopwatchExpire.Elapsed.ToString()));
      file.Close();
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
      PersistentCacheManager.Instance.CheckTablesCreated();
      RunPersistentExpiration();
    }
    protected async void RunPersistentExpiration()
    {
      if (_isRunning)
        return;
      if (Options.Expires == null)
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
      await PersistentCacheManager.Instance.DeleteAllExpired(typeof(T));
    }
  }
}
