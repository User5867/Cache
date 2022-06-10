using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs
{
  internal class Expiration
  {
    [ForeignKey(typeof(Value))]
    public object Object { get; set; }
    [OneToOne]
    public Value Value { get; set; }
    public DateTime Experation { get; set; }
  }
}
