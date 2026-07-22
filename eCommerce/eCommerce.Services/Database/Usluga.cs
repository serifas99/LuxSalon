using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eCommerce.Services.Database
{
    public class Usluga
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Naziv { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Opis { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cijena { get; set; }

        [Required]
        public int TrajanjeMinuta { get; set; }

        public int? UslugaKategorijaId { get; set; }

        [ForeignKey("UslugaKategorijaId")]
        public UslugaKategorija? UslugaKategorija { get; set; }

        // Tagovi za content-based recommendation (comma-separated, npr. "kosa,farbanje,zensko")
        [MaxLength(500)]
        public string Tagovi { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<FrizerUsluga> FrizerUsluge { get; set; } = new List<FrizerUsluga>();

        public ICollection<Termin> Termini { get; set; } = new List<Termin>();
    }
}
