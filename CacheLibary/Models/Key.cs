using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Internals;

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
    public override bool Equals(object obj)
    {
      return Equals(obj as IKey<K>);
    }
    public override int GetHashCode()
    {
      int hash = 13;
      hash = (hash * 17) + KeyIdentifier.GetHashCode();
      hash = (hash * 17) + KeyValue.GetHashCode();
      hash = (hash * 17) + ObjectType.FullName.GetHashCode();
      return hash;
    }
    public bool Equals(IKey<K> other)
    {
      return KeyIdentifier == other.KeyIdentifier && KeyValue.Equals(other.KeyValue) && ObjectType == other.ObjectType;
    }
  }
}
