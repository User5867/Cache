using CacheLibary.Interfaces;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models.Functions
{
  internal class HashFunctions
  {
    private static int _size = 104395337;
    private static SQLiteAsyncConnection _db = PersistentCacheManager.Instance.GetDatabase();

    private static HashFunctions Instance = new HashFunctions();

    private static int Mod(int a, int b)
    {
      return ((a % b) + b) % b;
    }
    public static int GetIndexByHash(int hash, int j)
    {
      return Mod(Mod(hash, _size) - j * (1 + Mod(hash, _size - 2)), _size);
    }
    internal static int GetFirstFreeIndex<D>(int hashcode) where D : IHash, new()
    {
      int j = 0;
      int index;
      D v;
      lock (Instance)
      {
        do
        {
          index = GetIndexByHash(hashcode, j);
          v = TryGetByIndex<D>(index).Result;
          j++;
        }
        while (v != null && !v.Deleted);
      }
      return index;
    }

    internal static async Task<D> TryGetByIndex<D>(int index) where D : new()
    {
      try
      {
        return await GetByIndex<D>(index);
      }
      catch
      {
        return default;
      }
    }
    public static async Task<D> GetByHashcode<D, T>(int hashcode, T value) where D : IHash, new()
    {
      Func<D, bool> equals = t => t.Equals(value);
      return await GetByHashcode<D, T>(hashcode, equals);
    }
    public static async Task<D> GetByHashcode<D, T>(int hashcode, Func<D, bool> Equals) where D : IHash, new()
    {
      int j = 0;
      D k;
      do
      {
        k = await TryGetByIndex<D>(GetIndexByHash(hashcode, j));
        j++;
      } while (k != null && !(!k.Deleted && k.Hashcode == hashcode && Equals(k)));
      return k;
    }

    private static async Task<D> GetByIndex<D>(int index) where D : new()
    {
      return await _db.GetWithChildrenAsync<D>(index);
    }
  }
}
