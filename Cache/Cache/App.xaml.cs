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
      bool b = System.Threading.ThreadPool.SetMaxThreads(50, 50);
    }

    protected override void OnStart()
    {
      Task.Run(async () =>
      {
        await CacheHelper.CacheHelper.InitApp();
        Device.BeginInvokeOnMainThread(() =>
        {
          NavigationController.Instance.PushModalAsync(new LoadData());
        });
        
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
