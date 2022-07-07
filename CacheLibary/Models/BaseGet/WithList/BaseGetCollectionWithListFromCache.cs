using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models.BaseGet
{
  abstract class BaseGetCollectionWithListFromCache<T, K> : BaseGetCollectionFromCache<T, IEnumerable<K>>
  {
    public T SingleValue { get; set; }
    public IKey<K> SingleKey { get; set; }
    internal BaseGetCollectionWithListFromCache(IOptions options) : base(options)
    {
    }
    
    protected override async Task<ICollection<T>> Get(IKey<IEnumerable<K>> key, bool offline = false)
    {
      List<T> values = new List<T>();
      ICollection<K> unfoundKeys = new List<K>();
      foreach (K k in key.KeyValue)
      {
        ClearPropertys();
        SingleKey = new Key<K>(key.KeyIdentifier, k, key.ObjectType);
        GetFromMemory();
        await GetFromPersistentAndSave();
        UpdateExpiration();
        if (ValueIsSet())
          values.Add(SingleValue);
        else
          unfoundKeys.Add(k);
      }
      if (unfoundKeys.Count == 0)
        return values;
      Key = new Key<IEnumerable<K>>(key.KeyIdentifier, unfoundKeys, key.ObjectType);
      if (!offline)
      {
        await GetFromServiceAndSave();
        values.AddRange(Value);
      }
      else
      {
        return new List<T>();
      }
      return values;
    }
    protected override async Task GetFromPersistentAndSave()
    {
      if (ValueIsSet())
        return;
      await GetFromPersistent();
      if (ValueIsSet())
        SaveOneToMemory();
    }
    protected override void GetFromMemory()
    {
      SingleValue = MemoryManager.Get<T, K>(SingleKey);
    }
    protected override async Task GetFromPersistent()
    {
      SingleValue = await PersistentManager.Get<T, K>(SingleKey);
    }
    protected override void SaveToPersistent()
    {
      foreach(T value in Value)
      {
        SingleValue = value;
        LoadKeyFromCurrentValue();
        SaveOneToPersistent();
      }
    }
    protected override void ClearPropertys()
    {
      SingleValue = default;
      SingleKey = null;
    }
    protected virtual void SaveOneToPersistent()
    {
      PersistentManager.Save(SingleKey, SingleValue, Options);
    }
    protected override void SaveToMemory()
    {
      foreach (T value in Value)
      {
        SingleValue = value;
        LoadKeyFromCurrentValue();
        SaveOneToMemory();
      }
    }
    protected virtual void SaveOneToMemory()
    {
      MemoryManager.Save(SingleKey, SingleValue, Options);
    }
    protected override void UpdateExpiration()
    {
      if (ValueIsSet())
        PersistentManager.UpdateExpiration(SingleKey);
    }
    protected override bool ValueIsSet()
    {
      if (base.ValueIsSet())
        return true;
      return !IsNull(SingleValue);
    }
    protected abstract void LoadKeyFromCurrentValue();
  }
}
