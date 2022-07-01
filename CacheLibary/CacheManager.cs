using CacheLibary.Interfaces;
using CacheLibary.Interfaces.CacheManager;
using CacheLibary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CacheLibary
{
  public class CacheManager : ICacheManager
  {
    private Dictionary<Type, ICache> _caches = new Dictionary<Type, ICache>();
    public static ICacheManager Instance { get; } = new CacheManager();
    private const string _cacheDirectory = "CacheLibary.CacheObjects";
    private CacheManager()
    {
      List<Type> cacheTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsInterface && t.Namespace == _cacheDirectory).ToList();
      foreach (Type i in cacheTypes)
      {
        Type customCache = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && t.GetInterface(i.Name) != null).FirstOrDefault();
        if (customCache == null)
          continue;
        object imp = Activator.CreateInstance(customCache);
        if (imp is ICache cache)
          _caches.Add(i, cache);
      }
    }
    public T GetCache<T>()
    {
      PersistentCacheManager.Instance.CheckTablesCreated();
      if (_caches[typeof(T)] is T cache)
        return cache;
      return default;
    }
  }
}
