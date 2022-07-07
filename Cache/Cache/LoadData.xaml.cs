using SysPro.PSM.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Cache
{
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class LoadData : BaseContentPage
  {
    public LoadData()
    {
      InitializeComponent();
      BindingContext = new LoadDataViewModel();
    }
  }
}