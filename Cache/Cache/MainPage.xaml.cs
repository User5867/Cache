using CacheLibary;
using CacheLibary.CacheObjects;
using CacheLibary.Interfaces.CacheManager;
using SysPro.Client.WebApi.Generated.Sprinter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Cache
{
  public partial class MainPage : ContentPage
  {
    public MainPage()
    {
      InitializeComponent();
      ICacheManager cacheManager = CacheManager.Instance;
      IMaterialCache materialCache = cacheManager.GetCache<IMaterialCache>();
      _ = Task.Run(async () =>
      {
        try
        {
        Material m = await materialCache.GetMaterialBySku("9300806190006");
        m = await materialCache.GetMaterialBySku("9300806190006");
        }
        catch(Exception e)
        {

        }
      });
      
    }
  }
}
