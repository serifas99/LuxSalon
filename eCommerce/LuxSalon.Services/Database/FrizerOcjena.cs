using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxSalon.Services.Database
{
    /// <summary>
    /// Ocjena koju klijent ostavlja frizeru nakon odradjenog termina.
    /// Koristi se kao popularity signal (prosjecna ocjena frizera) u sistemu preporuke - vidi RecommendationService.
    /// Jedna ocjena po terminu (unique constraint u konfiguraciji).
    /// </summary>
    public class FrizerOcjena
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TerminId { get; set; }

        [ForeignKey("TerminId")]
        public Termin Termin { get; set; } = null!;

        [Required]
        public int KlijentId { get; set; }

        [ForeignKey("KlijentId")]
        public User Klijent { get; set; } = null!;

        [Required]
        public int FrizerId { get; set; }

        [ForeignKey("FrizerId")]
        public Frizer Frizer { get; set; } = null!;

        [Required]
        [Range(1, 5)]
        public int Ocjena { get; set; }

        [MaxLength(500)]
        public string? Komentar { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
