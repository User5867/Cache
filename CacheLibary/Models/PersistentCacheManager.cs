using CacheLibary.DAOs;
using CacheLibary.DAOs.OptionDAOs;
using CacheLibary.Interfaces;
using CacheLibary.Interfaces.CacheManager;
using CacheLibary.Models.Functions;
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
  internal class PersistentCacheManager : IPersistentCacheManager
  {
    public static IPersistentCacheManager Instance { get; } = new PersistentCacheManager();
    private SQLiteAsyncConnection _db;
    private TaskCompletionSource<bool> _tableExists = new TaskCompletionSource<bool>();
    private bool _isLoading = true;

    public const int Deletet = -1;

    private PersistentCacheManager()
    {
      _db = new SQLiteAsyncConnection(DependencyService.Get<IFileHelper>().GetLocalFilePath("cache.db3"), SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.ReadWrite);
      _ = Task.Run(async () =>
        {
          try
          {
            await _db.RunInTransactionAsync((t) =>
            {
              _ = t.CreateTable<Value>();
              _ = t.CreateTable<Key>();
              _ = t.CreateTable<Expiration>();
              _ = t.CreateTable<KeyValue>();
              //_ = t.CreateTable<MaterialDAO>();
              _tableExists.SetResult(true);
              _isLoading = false;
            });
          }
          catch (Exception e)
          {

          }
        });
    }

    public SQLiteAsyncConnection GetDatabase()
    {
      return _db;
    }
    public async Task<T> Get<T, K>(IKey<K> key)
    {
      if (_isLoading)
      {
        bool t = _tableExists.Task.Result;
      }
      int hash = KeyFunctions.GetHashcode(key);
      return await ValueFunctions.GetValueByHashcode<T, K>(hash, key);
    }
    public async Task<T> Get<T, D, K>(IKey<K> key) where D : ICustomOptionDAO<T>, T, new()
    {
      if (_isLoading)
      {
        bool t = _tableExists.Task.Result;
      }
      int hash = KeyFunctions.GetHashcode(key);
      return await ValueFunctions.GetValueByHashcode<T, D, K>(hash, key);
    }

    public void Save<T, K>(IKey<K> key, T value, IOptions options)
    {
      ValueFunctions.SaveNewValue(key, value, options);
    }

    public Task<ICollection<T>> GetCollection<T, K>(IKey<K> key)
    {
      throw new NotImplementedException();
    }

    public void SaveCollection<T, K>(IKey<K> key, ICollection<T> values, IOptions options)
    {
      throw new NotImplementedException();
    }

    public Task<ICollection<T>> GetCollection<T, D, K>(IKey<K> key) where D : ICustomOptionDAO<T>, T, new()
    {
      throw new NotImplementedException();
    }

    public void Save<T, D, K>(IKey<K> key, T value, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      throw new NotImplementedException();
    }

    public void SaveCollection<T, D, K>(IKey<K> key, ICollection<T> values, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      throw new NotImplementedException();
    }
  }
}
