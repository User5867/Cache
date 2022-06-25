using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs
{
  internal class Value
  {
    [PrimaryKey]
    public int Id { get; set; }
    //TODO: Hashcode remove and Id to Autoincrement
    public int Hashcode { get; set; }
    [TextBlob(nameof(ObjectBlob))]
    public object Object { get; set; }
    [ManyToMany(typeof(KeyValue))]
    public List<Key> Keys { get; set; }
    public string ObjectBlob { get; set; }
  }
}
