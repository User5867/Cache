using CacheLibary.Interfaces;
using CacheLibary.Models;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs
{
  internal class Key : IHash
  {
    [PrimaryKey]
    public int Id { get; set; }
    public int Hashcode { get; set; }
    public bool Deleted { get; set; }
    [TextBlob(nameof(ObjectKeyBlob))]
    public Key<object> ObjectKey { get; set; }
    public string ObjectKeyBlob { get; set; }
    [ManyToMany(typeof(KeyValue))]
    public List<Value> Values { get; set; }
    [ForeignKey(typeof(Expiration))]
    public int ExpirationId { get; set; }
    [OneToOne]
    public Expiration Expiration { get; set; }
  }
}
