using CacheLibary.DAOs.OptionDAOs;
using CacheLibary.Interfaces;
using CacheLibary.Models;
using CacheLibary.Models.BaseGet;
using CacheLibary.Models.BaseGet.WithList;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Options.Material
{
  internal abstract class GetFromMaterialCache<K> : BaseGetFromCacheExternal<SysPro.Client.WebApi.Generated.Sprinter.Material, MaterialDAO, K>
  {
    internal GetFromMaterialCache(IOptions options) : base(options)
    {
    }
    
  }
  internal abstract class GetCollectionFromMaterialCache<K> : BaseGetCollectionFromCacheExternal<SysPro.Client.WebApi.Generated.Sprinter.Material, MaterialDAO, K>
  {
    internal GetCollectionFromMaterialCache(IOptions options) : base(options)
    {
    }
  }
  internal abstract class GetCollectionWithListFromMaterialCache<K> : BaseGetCollectionWithListFromCacheExternal<SysPro.Client.WebApi.Generated.Sprinter.Material, MaterialDAO, K>
  {
    internal GetCollectionWithListFromMaterialCache(IOptions options) : base(options)
    {
    }
  }
}
