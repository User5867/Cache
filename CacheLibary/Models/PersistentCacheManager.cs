using CacheLibary.DAOs;
using CacheLibary.Interfaces;
using CacheLibary.Interfaces.CacheManager;
using SQLite;
using SQLiteNetExtensions.Extensions;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CacheLibary.Models
{
  class PersistentCacheManager : IPersistentCacheManager
  {
    public static IPersistentCacheManager Instance { get; } = new PersistentCacheManager();
    private SQLiteAsyncConnection db;
    private TaskCompletionSource<bool> _tableExists = new TaskCompletionSource<bool>();
    private bool _isLoading = true;

    private const int deletet = -1;

    private PersistentCacheManager()
    {
      db = new SQLiteAsyncConnection(DependencyService.Get<IFileHelper>().GetLocalFilePath("cache.db3"), SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.ReadWrite);
      Task.Run(async () =>
      {
        try
        {
        await db.RunInTransactionAsync((t) =>
        {
          t.CreateTable<Value>();
          t.CreateTable<Key>();
          t.CreateTable<Expiration>();
          t.CreateTable<KeyValue>();
          _tableExists.SetResult(true);
          _isLoading = false;
        });
        }catch(Exception e)
        {

        }
      });
      
    }
    public async Task<T> Get<T, K>(IKey<K> key)
    {
      if (_isLoading)
      {
        bool t = _tableExists.Task.Result;
      }
      int hash = GetHashcode(key);
      return await GetValueByHashcode<T, K>(hash, key);
    }

    private static int GetHashcode<K>(IKey<K> key)
    {
      return key.GetHashCode();
    }

    private int _size = 104395337;
    private int _m = 104395303;
    private int GetIndexByHash(int hash, int j)
    {
      return Mod(Mod(hash, _size) - j * (1 + Mod(hash, _m)), _size);
    }

    private int Mod(int a, int b)
    {
      return ((a % b) + b) % b;
    }

    private async Task<T> GetValueByHashcode<T, K>(int hashcode, IKey<K> key)
    {
      Key k = await GetKeyByHashcode(hashcode, key);
      if (k == null)
      {
        return default;
      }
      if (k?.Values?.FirstOrDefault()?.Object is T retVal)
        return retVal;
      return default;
    }

    private async Task<Key> GetKeyByHashcode<K>(int hashcode, IKey<K> key)
    {
      int j = 0;
      Key k = await GetKeyByIndex(GetIndexByHash(hashcode, j));
      while (k != null || (k.Hashcode == hashcode && key.Equals(k.ObjectKey)))
      {
        j++;
        k = await GetKeyByIndex(GetIndexByHash(hashcode, j));
      }
      return k;
    }

    private async Task<Key> GetKeyByIndex(int v)
    {
      return await db.GetWithChildrenAsync<Key>(v);
    }

    public async void Save<T, K>(IKey<K> key, T value, IOptions options)
    {
      int hashcode = GetHashCode(value);
      Value v = await GetValueByHashcode(hashcode, value);
      if(v == null)
        SaveNewValue(key, value, options);
      else
        SaveKeyForValue(key, v, options);
    }

    private int GetHashCode<T>(T value)
    {
      return value.GetHashCode();
    }

    private async Task<Value> GetValueByHashcode<T>(int hashcode, T value)
    {
      int j = 0;
      Value k = await GetValueByIndex(GetIndexByHash(hashcode, j));
      while (k != null || (k.Hashcode == hashcode && value.Equals(k.Object)))
      {
        j++;
        k = await GetValueByIndex(GetIndexByHash(hashcode, j));
      }
      return k;
    }

    private async Task<Value> GetValueByIndex(int v)
    {
      return await db.GetWithChildrenAsync<Value>(v);
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
    private async Task<int> GetFirstFreeKeyIndex(int hashcode)
    {
      int j = 0;
      int index;
      Key k;
      do
      {
        index = GetIndexByHash(hashcode, j);
        k = await GetKeyByIndex(index);
        j++;
      }
      while (k != null && k.Hashcode != deletet);
      return index;
    }
    private async Task<Key> CreateAndGetKey<K>(IKey<K> key)
    {
      Key k = new Key() { ObjectKey = key, Hashcode = GetHashcode(key), Id = await GetFirstFreeKeyIndex(GetHashcode(key))};
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
        Value = value.Id,
        Key = key.Id
      };
      _ = await db.InsertAsync(keyValue);
      UpdateExperation(key, options);
    }

    private async void SaveNewValue<T, K>(IKey<K> key, T value, IOptions options)
    {
      Key k = await CreateAndGetKey(key);
      Value v = await CreateAndGetValue(value);
        
    }

    private async Task<Value> CreateAndGetValue<T>(T value)
    {
      Value v = new Value() { Hashcode = GetHashCode(value), Object = value, Id = GetFirstFreeValueIndex(GetHashCode(value)) };
      throw new NotImplementedException();
    }

    private int GetFirstFreeValueIndex(int v)
    {
      throw new NotImplementedException();
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
          Key = key.ObjectKey,
          Experation = DateTime.UtcNow.Add(options.Expires.PersitentExperation.Value)
        };
      else
        expiration = null;
      return expires.HasValue;
    }
  }
}
