using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxSalon.Services.Database
{
    public class Termin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KlijentId { get; set; }

        [ForeignKey("KlijentId")]
        public User Klijent { get; set; } = null!;

        [Required]
        public int FrizerId { get; set; }

        [ForeignKey("FrizerId")]
        public Frizer Frizer { get; set; } = null!;

        [Required]
        public int UslugaId { get; set; }

        [ForeignKey("UslugaId")]
        public Usluga Usluga { get; set; } = null!;

        [Required]
        public DateTime DatumVrijeme { get; set; }

        [Required]
        public int TrajanjeMinuta { get; set; }

        // Cijena u trenutku zakazivanja (snapshot - cijena usluge se moze kasnije promijeniti)
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cijena { get; set; }

        [Required]
        public TerminStatus Status { get; set; } = TerminStatus.Zakazan;

        [MaxLength(500)]
        public string? Napomena { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Placanje? Placanje { get; set; }
    }

    // State machine statusa termina: Zakazan -> Potvrdjen -> Odradjen
    //                                                 \-> Otkazan / NijeSeOdazvao
    public enum TerminStatus
    {
        Zakazan,
        Potvrdjen,
        Odradjen,
        Otkazan,
        NijeSeOdazvao
    }
}
