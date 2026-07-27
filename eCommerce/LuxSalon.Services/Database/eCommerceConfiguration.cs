using Microsoft.EntityFrameworkCore;

namespace LuxSalon.Services.Database
{
    public partial class ECommerceDbContext : DbContext
    {

        private void CreateConfiguration(ModelBuilder modelBuilder)
        {
            // Configure UserRole relationships
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(ur => ur.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(ur => ur.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add any additional model configurations here

            CreateLuxSalonConfiguration(modelBuilder);
        }

        private void CreateLuxSalonConfiguration(ModelBuilder modelBuilder)
        {
            // Usluga -> UslugaKategorija (optional)
            modelBuilder.Entity<Usluga>()
                .HasOne(u => u.UslugaKategorija)
                .WithMany(k => k.Usluge)
                .HasForeignKey(u => u.UslugaKategorijaId)
                .OnDelete(DeleteBehavior.SetNull);

            // Frizer -> User (1:1). Restrict da izbjegnemo brisanje Usera dok postoji Frizer profil.
            modelBuilder.Entity<Frizer>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // FrizerUsluga (many-to-many join)
            modelBuilder.Entity<FrizerUsluga>()
                .HasOne(fu => fu.Frizer)
                .WithMany(f => f.FrizerUsluge)
                .HasForeignKey(fu => fu.FrizerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FrizerUsluga>()
                .HasOne(fu => fu.Usluga)
                .WithMany(u => u.FrizerUsluge)
                .HasForeignKey(fu => fu.UslugaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Termin -> Klijent (User), Frizer, Usluga
            // Restrict na svim da izbjegnemo "multiple cascade paths" gresku u SQL Serveru
            // (Frizer je i sam vezan na User, pa bi cascade od Usera stvorio dvostruki put do Termina)
            modelBuilder.Entity<Termin>()
                .HasOne(t => t.Klijent)
                .WithMany()
                .HasForeignKey(t => t.KlijentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Termin>()
                .HasOne(t => t.Frizer)
                .WithMany(f => f.Termini)
                .HasForeignKey(t => t.FrizerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Termin>()
                .HasOne(t => t.Usluga)
                .WithMany(u => u.Termini)
                .HasForeignKey(t => t.UslugaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Placanje -> Termin (1:1) - ako se termin obrise, brise se i placanje
            modelBuilder.Entity<Placanje>()
                .HasOne(p => p.Termin)
                .WithOne(t => t.Placanje)
                .HasForeignKey<Placanje>(p => p.TerminId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notifikacija -> Korisnik (User), Termin (opciono)
            modelBuilder.Entity<Notifikacija>()
                .HasOne(n => n.Korisnik)
                .WithMany()
                .HasForeignKey(n => n.KorisnikId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notifikacija>()
                .HasOne(n => n.Termin)
                .WithMany()
                .HasForeignKey(n => n.TerminId)
                .OnDelete(DeleteBehavior.SetNull);

            // FrizerOcjena - jedna ocjena po terminu
            modelBuilder.Entity<FrizerOcjena>()
                .HasIndex(o => o.TerminId)
                .IsUnique();

            modelBuilder.Entity<FrizerOcjena>()
                .HasOne(o => o.Termin)
                .WithMany()
                .HasForeignKey(o => o.TerminId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FrizerOcjena>()
                .HasOne(o => o.Klijent)
                .WithMany()
                .HasForeignKey(o => o.KlijentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FrizerOcjena>()
                .HasOne(o => o.Frizer)
                .WithMany()
                .HasForeignKey(o => o.FrizerId)
                .OnDelete(DeleteBehavior.Restrict);

            // RadnoVrijeme
            modelBuilder.Entity<RadnoVrijeme>()
                .HasOne(r => r.Frizer)
                .WithMany()
                .HasForeignKey(r => r.FrizerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RadnoVrijeme>()
                .HasIndex(r => new { r.FrizerId, r.DanUSedmici })
                .IsUnique();

            // IstorijaStatusaTermina
            modelBuilder.Entity<IstorijaStatusaTermina>()
                .HasOne(h => h.Termin)
                .WithMany()
                .HasForeignKey(h => h.TerminId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IstorijaStatusaTermina>()
                .HasOne(h => h.PromijenioKorisnik)
                .WithMany()
                .HasForeignKey(h => h.PromijenioKorisnikId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
