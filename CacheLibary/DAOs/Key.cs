using CacheLibary.Models;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs
{
  internal class Key
  {
    [PrimaryKey]
    public int Id { get; set; }
    public int Hashcode { get; set; }
    [TextBlob(nameof(ObjectKeyBlob))]
    public Key<object> ObjectKey { get; set; }
    public string ObjectKeyBlob { get; set; }
    [ManyToMany(typeof(KeyValue))]
    public List<Value> Values { get; set; }
  }
}
