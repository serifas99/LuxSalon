using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eCommerce.Services.Database
{
    public class UslugaKategorija
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Naziv { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Opis { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation - usluge u ovoj kategoriji
        public ICollection<Usluga> Usluge { get; set; } = new List<Usluga>();
    }
}
