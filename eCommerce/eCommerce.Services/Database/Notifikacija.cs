using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eCommerce.Services.Database
{
    public class Notifikacija
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KorisnikId { get; set; }

        [ForeignKey("KorisnikId")]
        public User Korisnik { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Naslov { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Poruka { get; set; } = string.Empty;

        [Required]
        public NotifikacijaTip Tip { get; set; } = NotifikacijaTip.Opsta;

        public bool Procitano { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? TerminId { get; set; }

        [ForeignKey("TerminId")]
        public Termin? Termin { get; set; }
    }

    public enum NotifikacijaTip
    {
        PodsjetnikTermina,
        TerminPotvrdjen,
        TerminOtkazan,
        PlacanjeUspjesno,
        PlacanjeVraceno,
        Opsta
    }
}
