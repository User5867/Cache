using CacheLibary.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models.BaseGet
{
  abstract class BaseGetCollectionWithListFromCache<T, K> : BaseGetCollectionFromCache<T, IEnumerable<K>>
  {
    internal BaseGetCollectionWithListFromCache(IOptions options) : base(options)
    {
    }
    
    protected override async Task<ICollection<T>> Get(IKey<IEnumerable<K>> key)
    {
      ConcurrentBag<T> cvalues = new ConcurrentBag<T>();
      Stopwatch s = new Stopwatch();
      s.Start();
      List<IKey<K>> unfoundKeys = key.KeyValue.AsParallel().Select(k => (IKey<K>)new Key<K>(key.KeyIdentifier, k, key.ObjectType)).ToList();
      s.Stop();
      Debug.Write(s.Elapsed);
      _ = Parallel.ForEach(unfoundKeys.ToList(), k =>
        {
        T value = GetFromMemory(k);
        if (!ValueIsSet(value))
          return;
        cvalues.Add(value);
        _ = unfoundKeys.Remove(k);
      });
      List<T> values = cvalues.ToList();
      if (unfoundKeys.Count == 0)
        return values;
      IEnumerable<T> persistentValues = await GetFromPersistent(unfoundKeys);
      if(persistentValues.Count() > 0)
        values.AddRange(persistentValues);
      IEnumerable<IKey<K>> foundKeys = values.AsParallel().Select(v => LoadKeyFromCurrentValue(v));
      unfoundKeys = unfoundKeys.Except(foundKeys).ToList();
      SaveToMemory(null, values);
      UpdateExpiration(foundKeys);
      if (unfoundKeys.Count == 0)
        return values;
      Key<IEnumerable<K>> keys = new Key<IEnumerable<K>>(key.KeyIdentifier, unfoundKeys.AsParallel().Select(k => k.KeyValue), key.ObjectType);
      bool offline = false;
      if (!offline)
      {
        ICollection<T> serviceValues = null;

        int kvc = unfoundKeys.Count;
        if (false && kvc > 10000)
        {
          int c = kvc / 10000 + 1;
          List<IKey<IEnumerable<K>>> splitKeys = keys.KeyValue.AsParallel().Select((value, index) => new { index, value }).GroupBy(o => o.index % c).Select(e => (IKey<IEnumerable<K>>)new Key<IEnumerable<K>>(key.KeyIdentifier, e.Select(v => v.value), key.ObjectType)).ToList();
          Task<ICollection<T>>[] tasks = new Task<ICollection<T>>[c];
          List<T> ret = new List<T>();
          for (int i = 0; i < c; i++)
          {
            tasks[i] = Task.Run(() => GetFromService(splitKeys[i]));
            await Task.Delay(100);
          }
          Task.WaitAll(tasks);
          foreach (Task<ICollection<T>> t in tasks)
          {
            ret.AddRange(t.Result);
            
          }
          serviceValues = ret;
          SaveToCache(null, serviceValues);
        }
        else
        {
          serviceValues = await GetFromServiceAndSave(keys, null);
        }
        if (serviceValues == null && serviceValues.Count == 0)
          return new List<T>();
        values.AddRange(serviceValues);
      }
      else
      {
        return new List<T>();
      }
      return values;
    }

    protected T GetFromMemory(IKey<K> singleKey)
    {
      return MemoryManager.Get<T, K>(singleKey);
    }
    protected virtual async Task<IEnumerable<T>> GetFromPersistent(IEnumerable<IKey<K>> keys)
    {
      return await PersistentManager.GetCollection<T, K>(keys);
    }
    protected override async void SaveToPersistent(IKey<IEnumerable<K>> key, ICollection<T> value)
    {
      ConcurrentBag<KeyValuePair<IKey<K>, T>> keyValues = new ConcurrentBag<KeyValuePair<IKey<K>, T>>();
      _ = Parallel.ForEach(value, v =>
      {
        IKey<K> singleKey = LoadKeyFromCurrentValue(v);
        keyValues.Add(new KeyValuePair<IKey<K>, T>(singleKey, v));
      });
      await SaveToPersistent(keyValues);
    }

    protected virtual async Task SaveToPersistent(IEnumerable<KeyValuePair<IKey<K>, T>> keyValues)
    {
      await PersistentManager.SaveCollection(keyValues, Options);
    }

    protected override void SaveToMemory(IKey<IEnumerable<K>> key, ICollection<T> value)
    {
      System.Diagnostics.Debug.Write(1);
      _ = Parallel.ForEach(value, v =>
      {
        IKey<K> singleKey = LoadKeyFromCurrentValue(v);
        SaveToMemory(singleKey, v);
      });
      System.Diagnostics.Debug.Write(1);
    }
    protected virtual async void SaveToMemory(IKey<K> singleKey, T singleValue)
    {
      MemoryManager.Save(singleKey, singleValue, Options);
    }
    protected void UpdateExpiration(IEnumerable<IKey<K>> keys)
    {
      PersistentManager.UpdateExpirations(keys);
    }
    protected bool ValueIsSet(T singleValue)
    {
      return !IsNull(singleValue);
    }
    protected abstract IKey<K> LoadKeyFromCurrentValue(T singleValue);
  }
}
