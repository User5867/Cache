using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Models
{
  internal class Key<K> : IKey<K>
  {
    public string KeyIdentifier { get; }
    public K KeyValue { get; }
    public Type ObjectType { get; }
    internal Key(K key, string identifier, Type objectType)
    {
      KeyValue = key;
      KeyIdentifier = identifier;
      ObjectType = objectType;
    }
  }
}
