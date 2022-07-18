using CacheLibary.Interfaces;
using CacheLibary.Models;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs
{
  internal class Key
  {
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }
    [Ignore]
    public Key<object> ObjectKey { get; private set; }
    private string _objectKeyBlob;
    [Unique]
    public string ObjectKeyBlob
    {
      get => _objectKeyBlob;
      set
      {
        _objectKeyBlob = value;
        ObjectKey = JsonConvert.DeserializeObject<Key<object>>(ObjectKeyBlob);
      }
    }
    [Indexed]
    public int ExpirationId { get; set; }
    public bool Deleted { get; set; }
  }
}
