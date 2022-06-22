using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cache.Droid;
using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileHelper))]
namespace Cache.Droid
{
  public class FileHelper : IFileHelper
  {
    private string _localFilePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
    public string GetLocalFilePath(string path)
    {
      return Path.Combine(_localFilePath, path);
    }
  }
}