using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs
{
  internal class Value
  {
    [PrimaryKey][AutoIncrement]
    public int Id { get; set; }
    public string ObjectBlob { get; set; }
  }
}
