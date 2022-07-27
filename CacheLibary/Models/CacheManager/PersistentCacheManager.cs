using CacheLibary.DAOs;
using CacheLibary.DAOs.OptionDAOs;
using CacheLibary.Interfaces;
using CacheLibary.Interfaces.CacheManager;
using CacheLibary.Models.Functions;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

    private PersistentCacheManager()
    {
      _db = new SQLiteAsyncConnection(DependencyService.Get<IFileHelper>().GetLocalFilePath("cache.db3"), SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.ReadWrite);
      _ = Task.Run(async () =>
        {
          try
          {
            await _db.RunInTransactionAsync((t) =>
            {
              //_ = t.DropTable<Value>();
              //_ = t.DropTable<Key>();
              //_ = t.DropTable<Expiration>();
              //_ = t.DropTable<KeyValue>();
              //_ = t.DropTable<MaterialDAO>();
              _ = t.CreateTable<Value>();
              _ = t.CreateTable<Key>();
              _ = t.CreateTable<Expiration>();
              _ = t.CreateTable<KeyValue>();
              _ = t.CreateTable<MaterialDAO>();
              _tableExists.SetResult(true);
              Run();
              _isLoading = false;
            });
          }
          catch (Exception e)
          {

          }
        });
    }
    internal const int CheckInterval = 60000;
    private const string WriteToFile = "[{0}] {1} : {2}";
    private const string DeleteTime = "DeleteTest2.txt";
    private const string UpdateTime = "UpdateTest1.txt";
    private const string SaveOne = "SaveTest1.txt";
    private const string SaveMultiple = "SaveTest2.txt";
    private Stopwatch _stopwatchExpire = new Stopwatch();
    private async void Run()
    {
      while (true)
      {
        _stopwatchExpire.Restart();
        int count = await ExpirationFunctions.DeleteKeyAndExpiration();
        _stopwatchExpire.Stop();
        StreamWriter file = new StreamWriter(DependencyService.Get<IFileHelper>().GetLocalFilePath(DeleteTime), true);
        await file.WriteLineAsync(string.Format(WriteToFile, DateTime.UtcNow, count, _stopwatchExpire.Elapsed.ToString()));
        file.Close();
        await Task.Delay(CheckInterval);
      }
    }

    public SQLiteAsyncConnection GetDatabase()
    {
      return _db;
    }
    public async Task<T> Get<T, K>(IKey<K> key)
    {
      return await ValueFunctions.GetValue<T, K>(key);
    }

    public void CheckTablesCreated()
    {
      if (_isLoading)
      {
        _ = _tableExists.Task.Result;
      }
    }

    public async Task<T> Get<T, D, K>(IKey<K> key) where D : ICustomOptionDAO<T>, T, new()
    {
      return await ValueFunctions.GetValue<T, D, K>(key);
    }

    public async Task DeleteAllExpired<D>(Type objectType) where D : ICustomOptionDAO, new()
    {
      await ExpirationFunctions.DeleteExpired<D>(objectType);
    }
    public async Task DeleteAllExpired(Type objectType)
    {
      await ExpirationFunctions.DeleteExpired(objectType);
    }

    public async Task Save<T, K>(IKey<K> key, T value, IOptions options)
    {
      await Task.Run(() => {
        lock (Instance)
        {
          ValueFunctions.TrySaveValue(key, value, options).Wait();
        }
      });
    }

    public async Task<ICollection<T>> GetCollection<T, K>(IKey<K> key)
    {
      return await ValueFunctions.GetValues<T, K>(key);
    }
    public async Task<ICollection<T>> GetCollection<T, D, K>(IEnumerable<IKey<K>> key) where D : ICustomOptionDAO<T>, T, new()
    {
      return await ValueFunctions.GetValues<T, D, K>(key);
    }

    public async Task SaveCollection<T, K>(IKey<K> key, ICollection<T> values, IOptions options)
    {
      await Task.Run(() =>
      {
        lock (Instance)
        {
          ValueFunctions.TrySaveValues(key, values, options).Wait();
        }
      });
    }

    public async Task<ICollection<T>> GetCollection<T, D, K>(IKey<K> key) where D : ICustomOptionDAO<T>, T, new()
    {
      return await ValueFunctions.GetValues<T, D, K>(key);
    }

    public async Task Save<T, D, K>(IKey<K> key, T value, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      Stopwatch _stopwatchSaveOne = new Stopwatch();
      _stopwatchSaveOne.Restart();
      await Task.Run(() =>
      {
        lock (Instance)
        {
          ValueFunctions.TrySaveValue<T, D, K>(key, value, options).Wait();
        }
      });
      _stopwatchSaveOne.Stop();
      StreamWriter file = new StreamWriter(DependencyService.Get<IFileHelper>().GetLocalFilePath(SaveOne), true);
      await file.WriteLineAsync(string.Format(WriteToFile, DateTime.UtcNow, 1, _stopwatchSaveOne.Elapsed.ToString()));
      file.Close();
    }

    public async Task SaveCollection<T, D, K>(IKey<K> key, ICollection<T> values, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      await Task.Run(() =>
      {
        lock (Instance)
        {
          ValueFunctions.TrySaveValues<T, D, K>(key, values, options).Wait();
        }
      });
      
    }

    public async void UpdateExpiration<K>(IKey<K> key)
    {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Restart();
      await ExpirationFunctions.UpdateExpiration(key);
      stopwatch.Stop();
      lock (UpdateTime)
      {
        StreamWriter file = new StreamWriter(DependencyService.Get<IFileHelper>().GetLocalFilePath(UpdateTime), true);
        file.WriteLine(string.Format(WriteToFile, DateTime.UtcNow, 1, stopwatch.Elapsed.ToString()));
        file.Close();
      }
    }

    public async void UpdateExpirations<K>(IEnumerable<IKey<K>> keys)
    {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Restart();
      await ExpirationFunctions.UpdateExpiration(keys);
      stopwatch.Stop();
      lock (UpdateTime)
      {
        StreamWriter file = new StreamWriter(DependencyService.Get<IFileHelper>().GetLocalFilePath(UpdateTime), true);
        file.WriteLine(string.Format(WriteToFile, DateTime.UtcNow, keys.Count(), stopwatch.Elapsed.ToString()));
        file.Close();
      }
    }

    public async Task SaveCollection<T, D, K>(IEnumerable<KeyValuePair<IKey<K>, T>> keyValues, IOptions options) where D : ICustomOptionDAO<T>, T, new()
    {
      Stopwatch _stopwatchSaveMultiple = new Stopwatch();
      _stopwatchSaveMultiple.Start();
      await Task.Run(() =>
      {
        lock (Instance)
        {
          ValueFunctions.TrySaveValues<T, D, K>(keyValues, options).Wait();
        }
      });
      _stopwatchSaveMultiple.Stop();
      StreamWriter file = new StreamWriter(DependencyService.Get<IFileHelper>().GetLocalFilePath(SaveMultiple), true);
      await file.WriteLineAsync(string.Format(WriteToFile, DateTime.UtcNow, keyValues.ToList().Count, _stopwatchSaveMultiple.Elapsed.ToString()));
      file.Close();
    }

    public async Task SaveCollection<T, K>(IEnumerable<KeyValuePair<IKey<K>, T>> keyValues, IOptions options)
    {
      await Task.Run(() =>
      {
        lock (Instance)
        {
          ValueFunctions.TrySaveValues(keyValues, options).Wait();
        }
      });
    }

    public async Task<IEnumerable<T>> GetCollection<T, K>(IEnumerable<IKey<K>> keys)
    {
      return await ValueFunctions.GetValues<T, K>(keys);
    }
  }
}
