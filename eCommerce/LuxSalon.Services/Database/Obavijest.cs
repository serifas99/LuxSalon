using System;
using System.ComponentModel.DataAnnotations;

namespace LuxSalon.Services.Database
{
    /// <summary>
    /// Salonske obavijesti/vijesti (npr. akcije, novi radnici, promjena radnog vremena) -
    /// razlicito od licnih Notifikacija koje su vezane za pojedinacnog korisnika/termin.
    /// Prikazuju se svim klijentima na pocetnom ekranu mobilne aplikacije.
    /// </summary>
    public class Obavijest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Naslov { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Tekst { get; set; } = string.Empty;

        public string? SlikaBase64 { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
