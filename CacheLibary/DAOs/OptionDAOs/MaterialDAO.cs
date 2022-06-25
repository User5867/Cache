using CacheLibary.Interfaces;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SysPro.Client.WebApi.Generated.Sprinter;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheLibary.DAOs.OptionDAOs
{
  internal class MaterialDAO : Material, ICustomOptionDAO<Material>
  {
    [TextBlob(nameof(PropertyListBlob))]
    public new ICollection<MaterialProperty> PropertyList { get; set; }
    public string PropertyListBlob { get; set; }
    [TextBlob(nameof(EanListBlob))]
    public new ICollection<MaterialEAN> EanList { get; set; }
    public string EanListBlob { get; set; }
    public MaterialDAO(Material material)
    {
      Price = material.Price;
      QuantityUnit2 = material.QuantityUnit2;
      QuantityUnit = material.QuantityUnit;
      SubGroup = material.SubGroup;
      Group = material.Group;
      CustomerNumber = material.CustomerNumber;
      Ean = material.Ean;
      MaterialBrand = material.MaterialBrand;
      MaterialName2 = material.MaterialName2;
      MaterialName = material.MaterialName;
      MaterialNumber = material.MaterialNumber;
      PluCustomerNumber = material.PluCustomerNumber;
      PluNr = material.PluNr;
      Segment = material.Segment;
      PropertyList = material.PropertyList;
      EanList = material.EanList;
      ID = Ean.GetHashCode();
    }

    public MaterialDAO()
    {
    }

    [PrimaryKey]
    public int ID { get; set; }
    public override int GetHashCode()
    {
      return ID;
    }
    public override bool Equals(object obj)
    {
      return base.Equals(obj);
    }

    public bool Equals(Material other)
    {//TODO: add more comparer
      return Ean == other.Ean;
    }
  }
}
