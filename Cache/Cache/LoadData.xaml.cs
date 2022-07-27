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
    private LoadDataViewModel _viewModel;
    public LoadData()
    {
      InitializeComponent();
      BindingContext = _viewModel = new LoadDataViewModel();
      _viewModel.TestInfo = "start Task";
      _ = Task.Run(() =>
      {
        try
        {
          _viewModel.TestInfo = "in Task";
          Test t = new Test(_viewModel);
          _viewModel.TestInfo = "test created";
          t.Test3();
          _viewModel.TestInfo = "test finished";
        }catch(Exception e)
        {
          _viewModel.TestInfo = e.Message;
        }
       
      });
    }
  }
}