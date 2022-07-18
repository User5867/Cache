using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs
{
  internal class Expiration
  {
    [PrimaryKey][AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public int KeyId { get; set; }
    public DateTime? TotalExpiration { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    public DateTime? LastAccess { get; set; }
  }
}
