using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs
{
  internal class Expiration
  {
    [PrimaryKey][AutoIncrement]
    public int Id { get; set; }
    [ForeignKey(typeof(Key))]
    public int KeyId { get; set; }
    [OneToOne(CascadeOperations = CascadeOperation.CascadeInsert)]
    public Key Key { get; set; }
    public DateTime? TotalExpiration { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    public DateTime? LastAccess { get; set; }
  }
}
