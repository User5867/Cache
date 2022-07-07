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
    private enum KeyTypes { SKU, NAME, SKU_LIST }
    public MaterialCache()
    {
      _getFromCache.Add((int)KeyTypes.SKU, new MaterialByMaterialNumber(Options));
      _getCollectionFromCache.Add((int)KeyTypes.NAME, new MaterialByName(Options));
      _getCollectionFromCache.Add((int)KeyTypes.SKU_LIST, new MaterialByMaterialNumberList(Options));
    }

    public override IOptions Options => new MaterialOptions();

    public async Task<ICollection<SysPro.Client.WebApi.Generated.Sprinter.Material>> GetMaterialByName(string name)
    {
      return await _getCollectionFromCache[(int)KeyTypes.NAME].GetBaseGetFromCache<string>().Get(name);
    }

    public async Task<SysPro.Client.WebApi.Generated.Sprinter.Material> GetMaterialByMaterialNumber(string sku)
    {
      return await _getFromCache[(int)KeyTypes.SKU].GetBaseGetFromCache<string>().Get(sku);
    }

    public async Task<ICollection<SysPro.Client.WebApi.Generated.Sprinter.Material>> GetMaterialByMaterialNumberList(IEnumerable<string> sku)
    {
      return await _getCollectionFromCache[(int)KeyTypes.SKU_LIST].GetBaseGetFromCache<IEnumerable<string>>().Get(sku);
    }
  }
}
