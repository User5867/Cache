using CacheLibary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Models.Functions
{
  internal class HashFunctions
  {
    private static int _size = 104395337;
    private static int _m = 104395303;
    
    private static int Mod(int a, int b)
    {
      return ((a % b) + b) % b;
    }
    public static int GetIndexByHash(int hash, int j)
    {
      return Mod(Mod(hash, _size) - j * (1 + Mod(hash, _m)), _size);
    }
  }
}
