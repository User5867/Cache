using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces
{
  internal interface ICustomOptionDAO<T> : IEquatable<T>
  {
    int ID { get; set; }
  }
}
