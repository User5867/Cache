using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models
{
  internal abstract class BaseGetFromCache<T, D, K> : BaseGetFromCache<T, K> where D : T, ICustomOptionDAO<T>, new()
  {
    internal BaseGetFromCache(IOptions options) : base(options)
    {
    }

    protected override async Task GetFromPersistent()
    {
      Value = await PersistentManager.Get<T, D, K>(Key);
    }
    protected override void SaveToPersistent()
    {
      PersistentManager.Save<T, D, K>(Key, Value, Options);
    }
  }
  internal abstract class BaseGetCollectionFromCache<T, K> : BaseGetFromCache<ICollection<T>, K>, IBaseGetCollectionFromCache<T, K>
  {
    internal BaseGetCollectionFromCache(IOptions options) : base(options)
    {
    }
    protected override async Task GetFromPersistent()
    {
      Value = await PersistentManager.GetCollection<T, K>(Key);
    }
    protected override void SaveToPersistent()
    {
      PersistentManager.SaveCollection(Key, Value, Options);
    }
  }
  internal abstract class BaseGetCollectionFromCache<T, D, K> : BaseGetFromCache<ICollection<T>, K> where D : T, ICustomOptionDAO<T>, new()
  {
    internal BaseGetCollectionFromCache(IOptions options) : base(options)
    {
    }
    protected override async Task GetFromPersistent()
    {
      Value = await PersistentManager.GetCollection<T, D, K>(Key);
    }
    protected override void SaveToPersistent()
    {
      PersistentManager.SaveCollection<T, D, K>(Key, Value, Options);
    }
  }
}
