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

namespace CacheLibary.Options.Material.Functions
{
  internal class MaterialBySku : GetFromMaterialCache<string>
  {
    public MaterialBySku(IOptions options) : base(options)
    {
    }
    public override async Task<SysPro.Client.WebApi.Generated.Sprinter.Material> Get(string key)
    {
      return await Get(new MaterialKey<string>(key, "sku"));
    }

    public override IBaseGetFromCacheExternal<SysPro.Client.WebApi.Generated.Sprinter.Material, MaterialDAO, K1> GetBaseGetFromCache<K1>()
    {
      return new MaterialBySku();
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
      if (res.Success)
        Value = res.Material;
      //Value = Newtonsoft.Json.JsonConvert.DeserializeObject<SysPro.Client.WebApi.Generated.Sprinter.Material>("{\"segment\":\"0\",\"pluNr\":0,\"pluCustomerNumber\":0,\"materialNumber\":\"63395693\",\"materialName\":\"LS LAYERED NHEAD TEE\",\"materialName2\":\"GIRLS NAILHEAD TEE\",\"ean\":\"9300806190006\",\"group\":\"150.6\",\"quantityUnit\":\"ST\",\"minQuantity\":8.0,\"status\":1,\"createDate\":\"2022-05-17T12:35:54+00:00\",\"changeDate\":\"2022-05-17T13:26:49+00:00\",\"groupName\":\"KIDS-CLOTHING-GIRLSWEAR 1-9\",\"propertyList\":[{\"segment\":\"0\",\"classId\":\"0001\",\"property\":\"4066\",\"characteristicName\":\"KIDS-CLOTHING\",\"characteristic\":\"0\",\"value\":\"150\",\"createDate\":\"2022-05-17T08:36:00+00:00\",\"changeDate\":\"2022-05-17T13:22:08+00:00\",\"className\":\"Allg-Merkmale\",\"propertyName\":\"Artikelwarengruppe1\",\"propertyShortName\":\"PRODUCT_GROUP_1\",\"minusTolerance\":0,\"plusTolerance\":0,\"unit\":\"\",\"unitTolerance\":\"\",\"dataType\":4,\"merkType\":\"K\"},{\"segment\":\"0\",\"classId\":\"0001\",\"property\":\"4067\",\"characteristicName\":\"GIRLSWEAR 1-9\",\"characteristic\":\"0\",\"value\":\"6\",\"createDate\":\"2022-05-17T08:36:00+00:00\",\"changeDate\":\"2022-05-17T13:22:08+00:00\",\"className\":\"Allg-Merkmale\",\"propertyName\":\"Artikelwarengruppe2\",\"propertyShortName\":\"PRODUCT_GROUP_2\",\"minusTolerance\":0,\"plusTolerance\":0,\"unit\":\"\",\"unitTolerance\":\"\",\"dataType\":4,\"merkType\":\"K\"},{\"segment\":\"0\",\"classId\":\"0001\",\"property\":\"4068\",\"characteristicName\":\"DISCONTINUED K/DS\",\"characteristic\":\"0\",\"value\":\"6999999001\",\"createDate\":\"2022-05-17T08:36:00+00:00\",\"changeDate\":\"2022-05-17T13:22:08+00:00\",\"className\":\"Allg-Merkmale\",\"propertyName\":\"Artikelwarengruppe3\",\"propertyShortName\":\"PRODUCT_GROUP_3\",\"minusTolerance\":0,\"plusTolerance\":0,\"unit\":\"\",\"unitTolerance\":\"\",\"dataType\":4,\"merkType\":\"K\"},{\"segment\":\"0\",\"classId\":\"FA01\",\"property\":\"7005\",\"characteristicName\":\"99577\",\"characteristic\":\"0\",\"value\":\"99577\",\"createDate\":\"2022-05-17T08:36:00+00:00\",\"changeDate\":\"2022-05-17T13:22:08+00:00\",\"className\":\"Fashion-Merkmale\",\"propertyName\":\"Modell\",\"propertyShortName\":\"MODELL\",\"minusTolerance\":0,\"plusTolerance\":0,\"unit\":\"\",\"unitTolerance\":\"\",\"dataType\":1,\"merkType\":\"K\"},{\"segment\":\"0\",\"classId\":\"FA01\",\"property\":\"7020\",\"characteristicName\":\"8\",\"characteristic\":\"0\",\"value\":\"Clearance\",\"createDate\":\"2022-05-17T08:36:00+00:00\",\"changeDate\":\"2022-05-17T13:22:08+00:00\",\"className\":\"FA01\",\"propertyName\":\"KMART.KEYCODESTATUS\",\"propertyShortName\":\"KMART.KEYCODESTATUS\",\"minusTolerance\":0,\"plusTolerance\":0,\"unit\":\"\",\"unitTolerance\":\"0\",\"dataType\":0,\"merkType\":\"0\"},{\"segment\":\"0\",\"classId\":\"FA01\",\"property\":\"7023\",\"characteristicName\":\"CAGE\",\"characteristic\":\"0\",\"value\":\"CAGE\",\"createDate\":\"2022-05-17T08:36:00+00:00\",\"changeDate\":\"2022-05-17T13:22:08+00:00\",\"className\":\"FA01\",\"propertyName\":\"WT_BEZ\",\"propertyShortName\":\"WT_BEZ\",\"minusTolerance\":0,\"plusTolerance\":0,\"unit\":\"\",\"unitTolerance\":\"0\",\"dataType\":0,\"merkType\":\"0\"},{\"segment\":\"0\",\"classId\":\"FA01\",\"property\":\"7026\",\"characteristicName\":\"TABLE\",\"characteristic\":\"0\",\"value\":\"TABLE\",\"createDate\":\"2022-05-17T08:36:00+00:00\",\"changeDate\":\"2022-05-17T13:22:08+00:00\",\"className\":\"Fashion-Merkmale\",\"propertyName\":\"Warenplazierung\",\"propertyShortName\":\"PLACEMENT_AREA\",\"minusTolerance\":0,\"plusTolerance\":0,\"unit\":\"\",\"unitTolerance\":\"\",\"dataType\":4,\"merkType\":\"K\"}],\"eanList\":[{\"ean\":\"9300806190006\",\"createDate\":\"2022-05-17T08:36:12+00:00\",\"status\":1,\"sku\":\"\",\"propertyList\":[{\"classId\":\"E001\",\"property\":\"1001\",\"characteristicName\":\"GM LOVE\",\"characteristic\":\"0\",\"value\":\"GM LOVE\",\"createDate\":\"2022-05-17T08:36:12+00:00\",\"changeDate\":\"2022-05-17T13:25:39+00:00\",\"className\":\"EAN-Merkmale\",\"propertyName\":\"Farbe\",\"propertyShortName\":\"FARBE\",\"dataType\":4,\"ean\":\"9300806190006\"},{\"classId\":\"E001\",\"property\":\"1002\",\"characteristicName\":\"SIZE 2\",\"characteristic\":\"0\",\"value\":\"SIZE 2\",\"createDate\":\"2022-05-17T08:36:12+00:00\",\"changeDate\":\"2022-05-17T13:26:35+00:00\",\"className\":\"EAN-Merkmale\",\"propertyName\":\"Größe\",\"propertyShortName\":\"SIZE\",\"dataType\":4,\"ean\":\"9300806190006\"},{\"classId\":\"E001\",\"property\":\"1003\",\"characteristicName\":\"SIZE 2\",\"characteristic\":\"0\",\"value\":\"SIZE 2\",\"createDate\":\"2022-05-17T08:36:12+00:00\",\"changeDate\":\"2022-05-17T13:26:35+00:00\",\"className\":\"EAN-Merkmale\",\"propertyName\":\"Druck-Größe\",\"propertyShortName\":\"PRINT_SIZE\",\"dataType\":4,\"ean\":\"9300806190006\"},{\"classId\":\"E001\",\"property\":\"1017\",\"characteristicName\":\"N/A\",\"characteristic\":\"0\",\"value\":\"N/A\",\"createDate\":\"2022-05-17T08:36:12+00:00\",\"changeDate\":\"2022-05-17T13:26:35+00:00\",\"className\":\"EAN-Merkmale\",\"propertyName\":\"Warenträger\",\"propertyShortName\":\"TARGET_STORAGE_LAYER2\",\"dataType\":4,\"ean\":\"9300806190006\"}]}]}");
    }
  }
}
