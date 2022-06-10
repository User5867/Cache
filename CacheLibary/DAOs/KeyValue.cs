using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs
{
  internal class KeyValue
  {
    [ForeignKey(typeof(Key))]
    public object Key { get; set; }
    [ForeignKey(typeof(Value))]
    public object Object { get; set; }
  }
}
