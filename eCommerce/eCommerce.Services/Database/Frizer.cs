using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eCommerce.Services.Database
{
    public class Frizer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [MaxLength(1000)]
        public string? Biografija { get; set; }

        [MaxLength(200)]
        public string? Specijalizacija { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation - usluge koje frizer moze izvoditi (many-to-many)
        public ICollection<FrizerUsluga> FrizerUsluge { get; set; } = new List<FrizerUsluga>();

        // Navigation - termini kod ovog frizera
        public ICollection<Termin> Termini { get; set; } = new List<Termin>();
    }
}
