using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("exchange_rates")]
    public class ExchangeRate
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public int Id { get; set; }

        [Column("usd"), Required]
        public decimal Usd { get; set; }

        [Column("eur"), Required]
        public decimal Eur { get; set; }

        [Column("date"), Required]//, Index]
        public DateTime Date { get; set; }

        [Column("year"), Required]//, Index]
        public int Year { get; set; }

        [Column("month"), Required]//, Index]
        public int Month { get; set; }

        [Column("day"), Required]//, Index]
        public int Day { get; set; }
    }
}
