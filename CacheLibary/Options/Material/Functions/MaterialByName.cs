using Autofac;
using CacheLibary.Interfaces;
using CacheLibary.Models;
using SysPro.PSM.DependencyContainer;
using SysPro.PSM.Endpoints;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Options.Material.Functions
{
  internal class MaterialByName : GetCollectionFromMaterialCache<string>
  {
    public MaterialByName(IOptions options) : base(options)
    {
    }

    public override async Task<ICollection<SysPro.Client.WebApi.Generated.Sprinter.Material>> Get(string key)
    {
      return await Get(new MaterialKey<string>(key, "name"));
    }

    protected override async Task<ICollection<SysPro.Client.WebApi.Generated.Sprinter.Material>> GetFromService(IKey<string> Key)
    {
      var service = DepCon.Scope.Resolve<SysPro.Client.WebApi.Generated.Sprinter.ISPRINTER_Client>();
      var user = new UserInfoFac<SysPro.Client.WebApi.Generated.Sprinter.UserInfo>().Build();
      var recordFilter = new FilterFac<SysPro.Client.WebApi.Generated.Sprinter.RecordFilter>();
      recordFilter.Add("artbez", FilterComparer.Equal, Key.KeyValue);
      var res = await service.GetMaterialListAsync(new SysPro.Client.WebApi.Generated.Sprinter.GetMaterialListBody()
      {
        Userinfo = user,
        RecordFilter = recordFilter.Build()
      });
      if (res.Success)
        return res.MaterialList;
      return null;
    }
  }
}
