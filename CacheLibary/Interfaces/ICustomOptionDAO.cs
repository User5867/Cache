using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces
{
  internal interface ICustomOptionDAO<T> : ICustomOptionDAO, IEquatable<T>
  {
    D CreateInstance<D>(T value) where D : T, ICustomOptionDAO<T>;
  }
  internal interface ICustomOptionDAO : IHash
  {
  }
}
