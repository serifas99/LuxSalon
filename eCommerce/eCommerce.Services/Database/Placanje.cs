using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eCommerce.Services.Database
{
    public class Placanje
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TerminId { get; set; }

        [ForeignKey("TerminId")]
        public Termin Termin { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Iznos { get; set; }

        [Required]
        public PlacanjeStatus Status { get; set; } = PlacanjeStatus.NaCekanju;

        [MaxLength(100)]
        public string? PaypalOrderId { get; set; }

        [MaxLength(100)]
        public string? PaypalTransactionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DatumPlacanja { get; set; }

        public DateTime? DatumPovrata { get; set; }
    }

    public enum PlacanjeStatus
    {
        NaCekanju,
        Zavrseno,
        Vraceno,
        Neuspjesno
    }
}
