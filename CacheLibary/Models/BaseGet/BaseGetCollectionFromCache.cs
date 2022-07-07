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

    protected override async Task GetFromPersistent()
    {
      Value = await PersistentManager.GetCollection<T, K>(Key);
    }
    protected override void SaveToPersistent()
    {
      PersistentManager.SaveCollection(Key, Value, Options);
    }
    protected override bool ValueIsSet()
    {
      if (!base.ValueIsSet())
        return false;
      return Value.Any();
    }

  }
}
