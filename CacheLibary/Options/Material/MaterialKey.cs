﻿using CacheLibary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.Options.Material
{
  internal class MaterialKey<K> : Key<K>
  {
    internal MaterialKey(K key, string identifier) : base(key, identifier, typeof(SysPro.Client.WebApi.Generated.Sprinter.Material))
    {
    }
  }
}
