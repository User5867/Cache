using CacheHelper;
using Newtonsoft.Json;
using SysPro.PSM.Controls;
using SysPro.PSM.LocalStorage;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Cache
{
  public partial class App : Application
  {
    public App()
    {
      InitializeComponent();
      XF.Material.Forms.Material.Init(this);
      Current.MainPage = new MainPage();
      NavigationController.Instance.PushModalAsync(new LoadData());
      
    }

    protected override void OnStart()
    {
      Task.Run(async () =>
      {
        await CacheHelper.CacheHelper.InitApp();
      });
    }

    protected override void OnSleep()
    {
    }

    protected override void OnResume()
    {
    }
  }
}
