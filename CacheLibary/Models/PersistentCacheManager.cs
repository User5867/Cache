using CacheLibary.DAOs;
using CacheLibary.Interfaces;
using CacheLibary.Interfaces.CacheManager;
using SQLite;
using SQLiteNetExtensions.Extensions;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CacheLibary.Models
{
  class PersistentCacheManager : IPersistentCacheManager
  {
    public static IPersistentCacheManager Instance { get; } = new PersistentCacheManager();
    private SQLiteAsyncConnection db = new SQLiteAsyncConnection(DependencyService.Get<IFileHelper>().GetLocalFilePath("cache.db3"));
    private PersistentCacheManager()
    {

    }
    public Task<T> Get<T, K>(IKey<K> key)
    {
      db.GetAsync<Value>(key);
      throw new NotImplementedException();
    }
    public async void Save<T, K>(IKey<K> key, T value, IOptions options)
    {
      Value v = await db.GetWithChildrenAsync<Value>(value);
      if (v == null)
        SaveNewValue(key, value, options);
      else
        SaveKeyForValue(key, v, options);
    }

    private async void SaveKeyForValue<K>(IKey<K> key, Value value, IOptions options)
    {
      List<Key> keys = value.Keys;
      if (keys.Find(k => k.ObjectKey == key) is Key ke && ke != null)
        UpdateExperation(ke, options);
      else if (await db.GetAsync<Key>(key) is Key k && k != null)
        AddKeyToValue(k, value, options);
      else
        CreateKeyAndAddToValue(key, value, options);
    }
    private async Task<Key> CreateAndGetKey<K>(IKey<K> key)
    {
      Key k = new Key() { ObjectKey = key };
      _ = await db.InsertAsync(k);
      return k;
    }
    private async void CreateKeyAndAddToValue<K>(IKey<K> key, Value value, IOptions options)
    {
      Key k = await CreateAndGetKey(key);
      AddKeyToValue(k, value, options);
    }

    private async void AddKeyToValue(Key key, Value value, IOptions options)
    {
      KeyValue keyValue = new KeyValue()
      {
        Object = value.Object,
        Key = key.ObjectKey
      };
      _ = await db.InsertAsync(keyValue);
      UpdateExperation(key, options);
    }

    private void SaveNewValue<T, K>(IKey<K> key, T value, IOptions options)
    {
      

        
    }

    private void UpdateExperation(Key key, IOptions options)
    {
      if (TryGetPersitentExperation(key, options, out Expiration expiration))
      {
        _ = db.UpdateAsync(expiration);
      }
    }

    private bool TryGetPersitentExperation(Key key, IOptions options, out Expiration expiration)
    {
      TimeSpan? expires = options.Expires?.PersitentExperation;
      if (expires.HasValue)
        expiration = new Expiration()
        {
          Object = key.ObjectKey,
          Experation = DateTime.UtcNow.Add(options.Expires.PersitentExperation.Value)
        };
      else
        expiration = null;
      return expires.HasValue;
    }
  }
}
