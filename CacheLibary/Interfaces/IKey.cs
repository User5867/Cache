using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces
{
  internal interface IKey<K> : IEquatable<IKey<K>>
  {
    string KeyIdentifier { get; }
    K KeyValue { get; }
    Type ObjectType { get; }
  }
}
