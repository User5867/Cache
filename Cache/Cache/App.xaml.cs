using CacheHelper;
using Newtonsoft.Json;
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
      Task.Run(async () =>
      {
        await CacheHelper.CacheHelper.InitApp();
        MainPage = new MainPage();
      });
    }

    protected override void OnStart()
    {
    }

    protected override void OnSleep()
    {
    }

    protected override void OnResume()
    {
    }
  }
}
