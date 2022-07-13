using CacheLibary.Interfaces;
using CacheLibary.Interfaces.CacheManager;
using CacheLibary.Interfaces.Options;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CacheLibary.Models
{

  internal class MemoryCacheManager : IMemoryCacheManager
  {
    private MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 20000, TrackStatistics = true, ExpirationScanFrequency = TimeSpan.FromMinutes(1) });
    public static IMemoryCacheManager Instance = new MemoryCacheManager();

    private MemoryCacheManager()
    {
      ReadFromFile();
      Run();
    }

    public T Get<T, K>(IKey<K> key)
    {
      return _memoryCache.Get<T>(Key<K>.GetObjectKey(key));
    }

    public void Save<T, K>(IKey<K> key, T value, IOptions options)
    {
      long size = 1;
      if (value is ICollection c)
        size = c.Count;
      MemoryCacheEntryOptions entryOptions = new MemoryCacheEntryOptions() { Size = size, Priority = options.Priority.Priority };
      if (options.Expires is IExpires expires)
      {
        entryOptions.SlidingExpiration = expires.MemorySlidingExpiration;
        entryOptions.AbsoluteExpirationRelativeToNow = expires.MemoryExpiration;
      }
      _memoryCache.Set(Key<K>.GetObjectKey(key), value, entryOptions);
    }
    private bool _isRunning;
    private int _saveInterval = 10000;
    private async void Run()
    {
      if (_isRunning)
        return;
      _isRunning = true;
      while (_isRunning)
      {
        SaveToFile();
        await Task.Delay(_saveInterval);
      }
    }
    private string _fileName = DependencyService.Get<IFileHelper>().GetLocalFilePath("cache.txt");
    private void ReadFromFile()
    {
      if (!File.Exists(_fileName))
        return;
      string json = File.ReadAllText(_fileName);

      if (string.IsNullOrEmpty(json))
        return;
      ICollection<KeyValuePair<Key<object>, CustomCacheEntry>> keyValuePairs = JsonConvert.DeserializeObject<ICollection<KeyValuePair<Key<object>, CustomCacheEntry>>>(json);
      MethodInfo methodInfo = typeof(CacheExtensions).GetMethods().Where(m => m.Name == "Set").Where(m => m.IsGenericMethod).Where(m => m.IsStatic).FirstOrDefault(m => m.GetParameters().LastOrDefault().ParameterType == typeof(MemoryCacheEntryOptions));
      foreach (var entry in keyValuePairs)
      {
        Type t = entry.Key.ObjectType;
        Type tc = typeof(ICollection<>).MakeGenericType(t);
        MemoryCacheEntryOptions m = new MemoryCacheEntryOptions
        {
          AbsoluteExpiration = entry.Value.AbsoluteExpiration,
          Priority = entry.Value.Priority,
          Size = entry.Value.Size,
          SlidingExpiration = entry.Value.SlidingExpiration
        };
        m.ExpirationTokens.ToList().AddRange(entry.Value.ExpirationTokens);
        m.PostEvictionCallbacks.ToList().AddRange(entry.Value.PostEvictionCallbacks);
        MethodInfo mi = methodInfo.MakeGenericMethod(t);
        object v;
        try
        {
          v = JsonConvert.DeserializeObject(entry.Value.Value.ToString(), t);
        }
        catch
        {
          v = JsonConvert.DeserializeObject(entry.Value.Value.ToString(), tc);
          mi = methodInfo.MakeGenericMethod(tc);
        }         
        _ = mi.Invoke(_memoryCache, new object[] { _memoryCache, entry.Key, v, m });
      }
    }
    private void SaveToFile()
    {
      Type type = typeof(MemoryCache);
      FieldInfo fieldinfo = type.GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);
      if (fieldinfo == null)
        return;
      var f = fieldinfo.GetValue(_memoryCache);
      PropertyInfo propertyInfo = fieldinfo.FieldType.GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
      ICollection c = propertyInfo.GetValue(f) as ICollection;
      ICollection<KeyValuePair<object, ICacheEntry>> cacheCollectionValues = new List<KeyValuePair<object, ICacheEntry>>();

      foreach (var cI in c)
      {
        ICacheEntry cacheItemValue = cI.GetType().GetProperty("Value").GetValue(cI, null) as ICacheEntry;
        object cacheItemKey = cI.GetType().GetProperty("Key").GetValue(cI, null);
        cacheCollectionValues.Add(new KeyValuePair<object, ICacheEntry>(cacheItemKey, cacheItemValue));
      }
      string json = JsonConvert.SerializeObject(cacheCollectionValues);
      File.WriteAllText(_fileName, json);
    }
  }
}
