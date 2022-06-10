using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs
{
  internal class Value
  {
    [TextBlob(nameof(ObjectBlob))]
    [PrimaryKey]
    public object Object { get; set; }
    [ManyToMany(typeof(KeyValue))]
    public List<Key> Keys { get; set; }
    public string ObjectBlob { get; set; }
  }
}
