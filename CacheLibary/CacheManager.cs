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
      LoadCaches();
    }

    private void LoadCaches()
    {
      List<Type> cacheTypes = GetCacheInterfaces();
      foreach (Type interfaceType in cacheTypes)
      {
        Type customCache = GetClassTypeForInterface(interfaceType);
        if (customCache != null)
          CreateInstanceAndAddToCache(interfaceType, customCache);
      }
    }

    private static Type[] GetAssemblyTypes()
    {
      return Assembly.GetExecutingAssembly().GetTypes();
    }

    private static Type GetClassTypeForInterface(Type interfaceType)
    {
      Type[] assemblyTypes = GetAssemblyTypes();
      return assemblyTypes.Where(t => t.IsClass && t.GetInterface(interfaceType.Name) != null).FirstOrDefault();
    }

    private static List<Type> GetCacheInterfaces()
    {
      Type[] assemblyTypes = GetAssemblyTypes();
      return assemblyTypes.Where(t => t.IsInterface && t.Namespace == _cacheDirectory).ToList();
    }

    private void CreateInstanceAndAddToCache(Type interfaceType, Type customCache)
    {
      object imp = Activator.CreateInstance(customCache);
      if (imp is ICache cache)
        _caches.Add(interfaceType, cache);
    }

    public T GetCache<T>()
    {
      PersistentCacheManager.Instance.CheckTablesCreated();
      Type t = typeof(T);
      if (!_caches.ContainsKey(t))
        throw new KeyNotFoundException();
      if (_caches[t] is T cache)
        return cache;
      return default;
    }
  }
}
