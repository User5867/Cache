using Autofac;
using CacheLibary.Interfaces;
using SysPro.Client.WebApi.Generated.Sprinter;
using SysPro.PSM.DependencyContainer;
using SysPro.PSM.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.Options.Material.Functions
{
  internal class MaterialByMaterialNumberList : GetCollectionWithListFromMaterialCache<string>
  {
    public MaterialByMaterialNumberList(IOptions options) : base(options)
    {
    }

    public override async Task<ICollection<SysPro.Client.WebApi.Generated.Sprinter.Material>> Get(IEnumerable<string> key)
    {
      return await Get(new MaterialKey<IEnumerable<string>>(key, "materialNumber"));
    }
    protected override async Task<ICollection<SysPro.Client.WebApi.Generated.Sprinter.Material>> GetFromService(IKey<IEnumerable<string>> key)
    {
      if (key == null || !key.KeyValue.Any())
        return null;
      IEnumerable<string> keyValues = key.KeyValue;
      System.Diagnostics.Debug.Write("[KVC]" + keyValues.ToList().Count);
      RecordFilter recordFilter = new RecordFilter();
      string filter = string.Join(", ", keyValues.Select(m => $"'{m}'"));
      FilterFac<RecordFilter> filterFac = new FilterFac<RecordFilter>();
      filterFac.Add("materialnumber", FilterComparer.InGroup, filter);
      var res = await DepCon.Scope.Resolve<ISPRINTER_Client>().GetMaterialListAsync(
        new GetMaterialListBody
        {
          Userinfo = new UserInfoFac<UserInfo>().Build(),
          RecordFilter = filterFac.Build()
        }
        );
      System.Diagnostics.Debug.Write("[RMC]" + res.MaterialList.Count);
      if(res.MaterialList.Count != keyValues.Count())
      {
        List<SysPro.Client.WebApi.Generated.Sprinter.Material> r = res.MaterialList.ToList();
        ILookup<string, SysPro.Client.WebApi.Generated.Sprinter.Material> lu = r.ToLookup(m => m.MaterialNumber);
        List<SysPro.Client.WebApi.Generated.Sprinter.Material> doubled = r.Where(d => lu[d.MaterialNumber].Count() > 1).ToList();
        foreach(string m in doubled.Select(d => d.MaterialNumber).Distinct().ToList())
        {
          doubled.Remove(doubled.First(n => n.MaterialNumber == m));
        }
        foreach(var dou in doubled)
        {
          _ = res.MaterialList.Remove(dou);
        }
        
      }
      if (res.Success)
        return res.MaterialList;
      return null;
    }

    protected override IKey<string> LoadKeyFromCurrentValue(SysPro.Client.WebApi.Generated.Sprinter.Material singleValue)
    {
      return new MaterialKey<string>(singleValue.MaterialNumber, "materialNumber");
    }
  }
}
