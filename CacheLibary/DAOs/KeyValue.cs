using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs
{
  internal class KeyValue
  {
    [Indexed]
    public int KeyId { get; set; }
    [Indexed]
    public int ValueId { get; set; }
  }
}
