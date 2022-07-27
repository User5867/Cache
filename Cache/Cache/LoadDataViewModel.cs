using CacheLibary;
using CacheLibary.CacheObjects;
using CacheLibary.Interfaces.CacheManager;
using SysPro.Client.WebApi.Generated.Sprinter;
using SysPro.PSM.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Cache
{
  class LoadDataViewModel : BaseViewModel
  {
    private const string _materialByName = "name";
    private const string _materialByMaterialNumber = "materialNumber";
    private Stopwatch _stopwatch = new Stopwatch();
    enum SearchCriteria
    {
      MATERIAL_BY_NAME, MATERIAL_BY_MATERIAL_NUMBER
    }
    private Dictionary<string, SearchCriteria> _getCriteria = new Dictionary<string, SearchCriteria>();
    public ICollection<string> Criterias { get; set; } = new List<string>();
    private string _selectedCrtiteria;
    private string _testInfo;
    public string TestInfo { get => _testInfo; set => SetProperty(ref _testInfo, value); }
    public string SelectedCriteria
    {
      get => _selectedCrtiteria;
      set
      {
        if (string.IsNullOrEmpty(value))
          return;
        _selectedCrtiteria = value;
        _searchBy = _getCriteria[value];
        SetProperty(ref _selectedCrtiteria, value);
      }
    }
    private SearchCriteria _searchBy;
    public ICommand SearchCommand { get; set; }
    public string LoadTime { get => _stopwatch.Elapsed.ToString(); }
    public string Input { get; set; }
    private IEnumerable<Material> _materials = new List<Material>();
    public IEnumerable<Material> Materials
    {
      get => _materials;
      set => SetProperty(ref _materials, value);
    }
    private IMaterialCache _materialCache;
    public LoadDataViewModel()
    {
      FillCriterias();
      InitCommands();
      ICacheManager cacheManager = CacheManager.Instance;
      _materialCache = cacheManager.GetCache<IMaterialCache>();
    }

    private void InitCommands()
    {
      SearchCommand = new Command(() => Load());
    }
    private void StopTimer()
    {
      _stopwatch.Stop();
    }
    private void RunTimer()
    {
      if (_stopwatch.IsRunning)
        return;
      _stopwatch.Restart();
      _ = Task.Run(async () =>
      {
        while (_stopwatch.IsRunning)
        {
          OnPropertyChanged(nameof(LoadTime));
          await Task.Delay(10);
        }
      });

    }

    private void Load()
    {
      _ = Task.Run(async () =>
      {
        //bool list = true;
        RunTimer();
        ICollection<Material> ms;
        Material m;
        if (_searchBy == SearchCriteria.MATERIAL_BY_NAME)
          Materials = await _materialCache.GetMaterialByName(Input);
        if (_searchBy == SearchCriteria.MATERIAL_BY_MATERIAL_NUMBER)
          Materials = new List<Material>() { await _materialCache.GetMaterialByMaterialNumber(Input) };
        //if (list)
        //  try
        //  {
        //    await _materialCache.GetMaterialByMaterialNumberList(new List<string> { "61975828", "62042987", "62023566", "62145893" });
        //  }
        //  catch (Exception e)
        //  {

        //  }
        StopTimer();
      });
    }

    private void FillCriterias()
    {
      Criterias.Add(_materialByName);
      Criterias.Add(_materialByMaterialNumber);
      _getCriteria.Add(_materialByName, SearchCriteria.MATERIAL_BY_NAME);
      _getCriteria.Add(_materialByMaterialNumber, SearchCriteria.MATERIAL_BY_MATERIAL_NUMBER);
      SelectedCriteria = _materialByMaterialNumber;
    }
  }
}
