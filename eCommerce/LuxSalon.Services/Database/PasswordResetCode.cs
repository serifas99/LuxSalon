using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxSalon.Services.Database
{
    /// <summary>
    /// Jednokratni kod za "zaboravljena lozinka" tok (mobilna app). Kod se salje korisniku na email
    /// (preko RabbitMQ -> LuxSalon.Subscriber -> MailHog) i ovdje se cuva iskljucivo hashiran
    /// (isti PBKDF2 mehanizam kao za lozinke, preko ICryptoService), nikad u plain text obliku.
    /// Ima definisan istek (ExpiresAt) i moze se iskoristiti samo jednom (IsUsed).
    /// </summary>
    public class PasswordResetCode
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        public string CodeHash { get; set; } = string.Empty;

        [Required]
        public string CodeSalt { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
