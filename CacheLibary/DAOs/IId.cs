using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs
{
  internal interface IId
  {
    long ID { get; set; }
  }
}
