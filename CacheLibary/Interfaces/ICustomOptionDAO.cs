using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Interfaces
{
  internal interface ICustomOptionDAO<T> : ICustomOptionDAO
  {
    D CreateInstance<D>(T value) where D : T, ICustomOptionDAO<T>;
  }
  internal interface ICustomOptionDAO
  {
    int Id { get; set; }
    string UniqueId { get; set; }
  }
}
