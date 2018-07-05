using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
  [Table("cache")]
  public class Cache
  {
    [Key]
    [Column("key"), MaxLength(100)]
    public string Key { get; set; }

    [Column("value")]
    public string Value { get; set; } 

  }
}
