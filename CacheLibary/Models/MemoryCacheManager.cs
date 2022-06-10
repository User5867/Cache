using CacheLibary.Interfaces;
using CacheLibary.Interfaces.CacheManager;
using CacheLibary.Interfaces.Options;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CacheLibary.Models
{
  internal class MemoryCacheManager : IMemoryCacheManager
  {
    private MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 20000, TrackStatistics = true });
    private delegate Dictionary<IKey<object>, object> GetCache();
    public static IMemoryCacheManager Instance = new MemoryCacheManager();

    private MemoryCacheManager()
    {
      
    }
    public T TryGet<T, K>(IKey<K> key)
    {
      return _memoryCache.Get<T>(key);
    }

    public T Get<T, K>(IKey<K> key)
    {
      return _memoryCache.Get<T>(key);
    }

    public void Save<T, K>(IKey<K> key, T value, IOptions options)
    {
      MemoryCacheEntryOptions entryOptions = new MemoryCacheEntryOptions() { Size = 1, Priority = options.Priority.Priority};
      if (options.Expires is IExpires expires)
      {
        entryOptions.SlidingExpiration = expires.MemorySlidingExpiration;
        entryOptions.AbsoluteExpirationRelativeToNow = expires.MemoryExpiration;
      }

      _memoryCache.Set(key, value, entryOptions);
    }
    private bool _isRunning;
    private int _saveInterval;
    private async void Run()
    {
      if (_isRunning)
        return;
      while (_isRunning)
      {
        SaveToFile();
        await Task.Delay(_saveInterval);
      }
    }
    private string _fileName = DependencyService.Get<IFileHelper>().GetLocalFilePath("cache.txt");
    private void SaveToFile()
    {
      string json = null;
      try
      {
        json = File.ReadAllText(_fileName);
      }
      catch (Exception e)
      {

      }
      if (string.IsNullOrEmpty(json))
        return;
      Dictionary<IKey<object>, object> dic = new Dictionary<IKey<object>, object>();
      try
      {
        dic = JsonConvert.DeserializeObject<Dictionary<IKey<object>, object>>(json);
      }
      catch (Exception e)
      {

      }
      foreach(var entry in dic)
      {
        //useReflection to get Type
      }
            throw new NotImplementedException();
    }
  }
}
