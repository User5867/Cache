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
    Task<ICollection<T>> GetCollection<T, D, K>(IEnumerable<IKey<K>> key) where D : ICustomOptionDAO<T>, T, new();
    Task Save<T, K>(IKey<K> key, T value, IOptions options);
    Task Save<T, D, K>(IKey<K> key, T value, IOptions options) where D : ICustomOptionDAO<T>, T, new();
    Task SaveCollection<T, K>(IKey<K> key, ICollection<T> values, IOptions options);
    Task SaveCollection<T, D, K>(IKey<K> key, ICollection<T> values, IOptions options) where D : ICustomOptionDAO<T>, T, new();
    Task DeleteAllExpired<D>(Type objectType) where D : ICustomOptionDAO, new();
    Task DeleteAllExpired(Type objectType);
    void CheckTablesCreated();
    SQLiteAsyncConnection GetDatabase();
    void UpdateExpiration<K>(IKey<K> key);
    Task SaveCollection<T, D, K>(IEnumerable<KeyValuePair<IKey<K>, T>> keyValues, IOptions options) where D : ICustomOptionDAO<T>, T, new();
    Task SaveCollection<T, K>(IEnumerable<KeyValuePair<IKey<K>, T>> keyValues, IOptions options);
    Task<IEnumerable<T>> GetCollection<T, K>(IEnumerable<IKey<K>> keys);
  }
}
