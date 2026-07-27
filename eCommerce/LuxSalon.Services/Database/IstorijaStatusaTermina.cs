using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxSalon.Services.Database
{
    /// <summary>
    /// Audit trag promjena statusa termina - ko je promijenio status, kada, sa kojeg na koji,
    /// i eventualan opis/razlog. Popunjava se automatski iz TerminService pri svakoj promjeni statusa.
    /// </summary>
    public class IstorijaStatusaTermina
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TerminId { get; set; }

        [ForeignKey("TerminId")]
        public Termin Termin { get; set; } = null!;

        [Required]
        public TerminStatus PrethodniStatus { get; set; }

        [Required]
        public TerminStatus NoviStatus { get; set; }

        [Required]
        public int PromijenioKorisnikId { get; set; }

        [ForeignKey("PromijenioKorisnikId")]
        public User PromijenioKorisnik { get; set; } = null!;

        [MaxLength(500)]
        public string? Opis { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
