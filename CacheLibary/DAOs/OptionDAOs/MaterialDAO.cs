using CacheLibary.Interfaces;
using Newtonsoft.Json;
using SQLite;
using SysPro.Client.WebApi.Generated.Sprinter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CacheLibary.DAOs.OptionDAOs
{
  internal class MaterialDAO : Material, ICustomOptionDAO<Material>
  {
    [Ignore]
    public new ICollection<MaterialProperty> PropertyList { get; set; }
    private string _propertyListBlob;
    public string PropertyListBlob
    {
      get => _propertyListBlob;
      set
      {
        _propertyListBlob = value;
        base.PropertyList = JsonConvert.DeserializeObject<ICollection<MaterialProperty>>(PropertyListBlob);
      }
    }
    [Ignore]
    public new ICollection<MaterialEAN> EanList { get; set; }
    private string _eanListBlob;
    public string EanListBlob
    {
      get => _eanListBlob;
      set
      {
        _eanListBlob = value;
        base.EanList = JsonConvert.DeserializeObject<ICollection<MaterialEAN>>(EanListBlob);
      }
    }
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
      EanListBlob = JsonConvert.SerializeObject(material.EanList);
      PropertyListBlob = JsonConvert.SerializeObject(material.PropertyList);
      UniqueId = MaterialNumber;
    }

    public MaterialDAO()
    {
    }

    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }
    [Unique]
    public string UniqueId { get; set; }

    public D CreateInstance<D>(Material value) where D : Material, ICustomOptionDAO<Material>
    {
      return new MaterialDAO(value) as D;
    }
  }
}
