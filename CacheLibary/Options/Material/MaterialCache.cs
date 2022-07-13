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
    private enum KeyTypes { MATERIAL_NUMBER, NAME, MATERIAL_NUMBER_LIST }
    public MaterialCache()
    {
      _getFromCache.Add((int)KeyTypes.MATERIAL_NUMBER, new MaterialByMaterialNumber(Options));
      _getCollectionFromCache.Add((int)KeyTypes.NAME, new MaterialByName(Options));
      _getCollectionFromCache.Add((int)KeyTypes.MATERIAL_NUMBER_LIST, new MaterialByMaterialNumberList(Options));
    }

    public override IOptions Options => new MaterialOptions();

    public async Task<ICollection<SysPro.Client.WebApi.Generated.Sprinter.Material>> GetMaterialByName(string name)
    {
      return await _getCollectionFromCache[(int)KeyTypes.NAME].GetBaseGetFromCache<string>().Get(name);
    }

    public async Task<SysPro.Client.WebApi.Generated.Sprinter.Material> GetMaterialByMaterialNumber(string materialNumber)
    {
      return await _getFromCache[(int)KeyTypes.MATERIAL_NUMBER].GetBaseGetFromCache<string>().Get(materialNumber);
    }

    public async Task<ICollection<SysPro.Client.WebApi.Generated.Sprinter.Material>> GetMaterialByMaterialNumberList(IEnumerable<string> materialNumbers)
    {
      return await _getCollectionFromCache[(int)KeyTypes.MATERIAL_NUMBER_LIST].GetBaseGetFromCache<IEnumerable<string>>().Get(materialNumbers);
    }
  }
}
