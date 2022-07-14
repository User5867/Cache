using CacheLibary.Interfaces;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SysPro.Client.WebApi.Generated.Sprinter;
using System;
using System.Collections.Generic;
using System.Linq;
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
      Hashcode = MaterialNumber.GetHashCode();
    }

    public MaterialDAO()
    {
    }

    [PrimaryKey]
    public int Id { get; set; }
    public int Hashcode { get; set; }
    public bool Deleted { get; set; }

    public override int GetHashCode()
    {
      return Hashcode;
    }
    public override bool Equals(object obj)
    {
      return Equals(obj as Material);
    }

    public bool Equals(Material other)
    {//TODO: add more comparer
      if (other == null)
        return false;
      return MaterialNumber == other.MaterialNumber;
    }

    public D CreateInstance<D>(Material value) where D : Material, ICustomOptionDAO<Material>
    {
      return new MaterialDAO(value) as D;
    }
  }
}
