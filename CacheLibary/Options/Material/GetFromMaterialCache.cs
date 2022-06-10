using CacheLibary.Interfaces;
using CacheLibary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Options.Material
{
  internal abstract class GetFromMaterialCache<K> : BaseGetFromCache<SysPro.Client.WebApi.Generated.Sprinter.Material, K>
  {
    internal GetFromMaterialCache(IOptions options) : base(options)
    {
    }
  }
}
