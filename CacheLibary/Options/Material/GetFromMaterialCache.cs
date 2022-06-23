using CacheLibary.DAOs.OptionDAOs;
using CacheLibary.Interfaces;
using CacheLibary.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Options.Material
{
  internal abstract class GetFromMaterialCache<K> : BaseGetFromCache<SysPro.Client.WebApi.Generated.Sprinter.Material, K>
  {
    internal GetFromMaterialCache(IOptions options) : base(options)
    {
    }
    //protected override async Task GetFromPersistent()
    //{
    //  Value = await PersistentManager.Get<SysPro.Client.WebApi.Generated.Sprinter.Material, MaterialDAO, K>(Key);
    //}

    //protected override void SaveToPersistent()
    //{
    //  PersistentManager.Save<SysPro.Client.WebApi.Generated.Sprinter.Material, MaterialDAO, K>(Key, Value, Options);
    //}
  }
  internal abstract class GetCollectionFromMaterialCache<K> : BaseGetCollectionFromCache<SysPro.Client.WebApi.Generated.Sprinter.Material, K>
  {
    internal GetCollectionFromMaterialCache(IOptions options) : base(options)
    {
    }
  }
}
