using CacheLibary.CacheObjects;
using CacheLibary.DAOs.OptionDAOs;
using CacheLibary.Interfaces;
using CacheLibary.Models;
using CacheLibary.Options.Material.Functions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Options.Material
{
  internal class MaterialCache : Cache<SysPro.Client.WebApi.Generated.Sprinter.Material, MaterialDAO>, IMaterialCache
  {
    private GetCollectionFromMaterialCache<string> _byName;
    private GetFromMaterialCache<string> _bySku;
    public MaterialCache()
    {
      _getFromCache.Add("sku", new MaterialBySku(Options));
      _byName = new MaterialByName(Options);
      _bySku = new MaterialBySku(Options);
    }

    public override IOptions Options => new MaterialOptions();

    public async Task<ICollection<SysPro.Client.WebApi.Generated.Sprinter.Material>> GetMaterialByName(string name)
    {
      return await _byName.Get(name);
    }

    public async Task<SysPro.Client.WebApi.Generated.Sprinter.Material> GetMaterialBySku(string sku)
    {
      return await _bySku.Get(sku);
    }
  }
}
