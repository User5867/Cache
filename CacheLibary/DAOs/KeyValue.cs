using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs
{
  internal class KeyValue
  {
    [ForeignKey(typeof(Key))]
    public int KeyId { get; set; }
    [ForeignKey(typeof(Value))]
    public int ValueId { get; set; }
  }
}
