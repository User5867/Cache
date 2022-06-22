using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs
{
  internal class Expiration
  {
    [TextBlob(nameof(KeyBlob))]
    public object Key { get; set; }
    [ForeignKey(typeof(Key))]
    public string KeyBlob { get; set; }
    [OneToOne]
    public Value Value { get; set; }
    public DateTime Experation { get; set; }
    public DateTime LastAccess { get; set; }
  }
}
