using CacheLibary.Interfaces;
using CacheLibary.Models;
using SysPro.PSM.DependencyContainer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using SysPro.PSM.Endpoints;
using CacheLibary.DAOs.OptionDAOs;
using SysPro.Client.WebApi.Generated.Sprinter;
using System.Linq;

namespace CacheLibary.Options.Material.Functions
{
  internal class MaterialByMaterialNumber : GetFromMaterialCache<string>
  {
    public MaterialByMaterialNumber(IOptions options) : base(options)
    {
    }
    public override async Task<SysPro.Client.WebApi.Generated.Sprinter.Material> Get(string key)
    {
      return await Get(new MaterialKey<string>(key, "materialNumber"));
    }
    protected override async Task<SysPro.Client.WebApi.Generated.Sprinter.Material> GetFromService(IKey<string> key)
    {
      var res = await DepCon.Scope.Resolve<ISPRINTER_Client>()
      .GetMaterialByIdentifierAsync(new GetMaterialByIdentifierBody
      {
        Userinfo = new UserInfoFac<UserInfo>().Build(),
        MaterialIndentifierType = MaterialIndentifierTypes._0,
        Value = key.KeyValue
      });
      if (res.Success)
        return res.Material;
      return null;
    }
  }
}
