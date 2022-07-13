using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models.BaseGet
{
  internal abstract class BaseGetCollectionFromCache<T, K> : BaseGetFromCache<ICollection<T>, K>, IBaseGetCollectionFromCache<T, K>
  {
    internal BaseGetCollectionFromCache(IOptions options) : base(options)
    {
    }

    protected override async Task<ICollection<T>> GetFromPersistent(IKey<K> key)
    {
      return await PersistentManager.GetCollection<T, K>(key);
    }
    protected override void SaveToPersistent(IKey<K> key, ICollection<T> value)
    {
      PersistentManager.SaveCollection(key, value, Options);
    }
    protected override bool ValueIsSet(ICollection<T> value)
    {
      if (!base.ValueIsSet(value))
        return false;
      return value.Any();
    }

  }
}
