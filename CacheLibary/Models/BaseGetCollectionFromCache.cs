using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Models
{
  internal abstract class BaseGetCollectionFromCache<T, K> : BaseGetFromCache<ICollection<T>, K>
  {
    internal BaseGetCollectionFromCache(IOptions options) : base(options)
    {
    }
    protected override Task GetFromPersistent()
    {
      return base.GetFromPersistent();
    }
    protected override void SaveToPersistent()
    {
      base.SaveToPersistent();
    }
  }
}
