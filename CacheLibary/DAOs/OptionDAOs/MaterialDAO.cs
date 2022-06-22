using SQLite;
using SysPro.Client.WebApi.Generated.Sprinter;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs.OptionDAOs
{
  internal class MaterialDAO : Material, IId
  {
    [AutoIncrement]
    [PrimaryKey]
    public long ID { get; set; }
  }
}
