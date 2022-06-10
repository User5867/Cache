using CacheLibary.Interfaces;
using CacheLibary.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Options.Material.Functions
{
  internal class MaterialByName : GetFromMaterialCache<string>
  {
    public MaterialByName(IOptions options) : base(options)
    {
    }

    public override async Task<SysPro.Client.WebApi.Generated.Sprinter.Material> Get(string key)
    {
      return await Get(new MaterialKey<string>(key, "name"));
    }

    protected override Task GetFromService()
    {
      throw new NotImplementedException();
    }
  }
}
