using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxSalon.Services.Database
{
    /// <summary>
    /// Radno vrijeme frizera po danu u sedmici. Koristi se za racunanje stvarno dostupnih
    /// termina (color-coded kalendar u mobilnoj aplikaciji) - kombinuje se sa vec zakazanim
    /// terminima da se izracuna koji slotovi su slobodni.
    /// </summary>
    public class RadnoVrijeme
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FrizerId { get; set; }

        [ForeignKey("FrizerId")]
        public Frizer Frizer { get; set; } = null!;

        [Required]
        public DayOfWeek DanUSedmici { get; set; }

        [Required]
        public TimeSpan PocetakRada { get; set; }

        [Required]
        public TimeSpan KrajRada { get; set; }

        public bool NeRadi { get; set; } = false;
    }
}
