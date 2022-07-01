using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces
{
  interface IHash
  {
    int Id { get; set; }
    int Hashcode { get; set; }
    bool Deleted { get; set; }
  }
}
