using CacheLibary.DAOs;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Interfaces.CacheManager
{
  interface IPersistentCacheManager
  {
    Task<T> Get<T, K>(IKey<K> key);
    Task<T> Get<T, D, K>(IKey<K> key) where D : ICustomOptionDAO<T>, T, new();
    Task<ICollection<T>> GetCollection<T, K>(IKey<K> key);
    Task<ICollection<T>> GetCollection<T, D, K>(IKey<K> key) where D : ICustomOptionDAO<T>, T, new();
    void Save<T, K>(IKey<K> key, T value, IOptions options);
    void Save<T, D, K>(IKey<K> key, T value, IOptions options) where D : ICustomOptionDAO<T>, T, new();
    void SaveCollection<T, K>(IKey<K> key, ICollection<T> values, IOptions options);
    void SaveCollection<T, D, K>(IKey<K> key, ICollection<T> values, IOptions options) where D : ICustomOptionDAO<T>, T, new();
    SQLiteAsyncConnection GetDatabase();
  }
}
