using SysPro.Client.WebApi.Generated.Sprinter;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheLibary.CacheObjects
{
  public interface IMaterialCache
  {
    Task<ICollection<Material>> GetMaterialByName(string name);
    Task<ICollection<Material>> GetMaterialByMaterialNumberList(IEnumerable<string> materialNumber);
    Task<Material> GetMaterialByMaterialNumber(string materialNumber);
  }
}
