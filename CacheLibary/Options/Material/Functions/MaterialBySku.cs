using CacheLibary.Interfaces;
using CacheLibary.Models;
using SysPro.PSM.DependencyContainer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using SysPro.PSM.Endpoints;

namespace CacheLibary.Options.Material.Functions
{
  internal class MaterialBySku : GetFromMaterialCache<string>
  {
    public MaterialBySku(IOptions options) : base(options)
    {
    }

    public override async Task<SysPro.Client.WebApi.Generated.Sprinter.Material> Get(string key)
    {
      try
      {
        return await Get(new MaterialKey<string>(key, "sku"));
      }
      catch (Exception e)
      {

      }
      return await Get(new MaterialKey<string>(key, "sku"));
    }
    protected override async Task GetFromService()
    {
      var service = DepCon.Scope.Resolve<SysPro.Client.WebApi.Generated.Sprinter.ISPRINTER_Client>();
      var user = new UserInfoFac<SysPro.Client.WebApi.Generated.Sprinter.UserInfo>().Build();
      var res = await service.GetMaterialByIdentifierAsync(new SysPro.Client.WebApi.Generated.Sprinter.GetMaterialByIdentifierBody
      {
        Userinfo = user,
        MaterialIndentifierType = SysPro.Client.WebApi.Generated.Sprinter.MaterialIndentifierTypes._1,
        Value = Key.KeyValue
      });
      if(res.Success)
        Value = res.Material;
    }
  }
}
