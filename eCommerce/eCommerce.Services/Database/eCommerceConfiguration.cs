using Microsoft.EntityFrameworkCore;

namespace eCommerce.Services.Database
{
    public partial class ECommerceDbContext : DbContext
    {

        private void CreateConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ProductCategory relationships
            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Product)
                .WithMany(pc => pc.ProductCategories)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Category)
                .WithMany(pc => pc.ProductCategories)
                .HasForeignKey(pc => pc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

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

            modelBuilder.Entity<Asset>()
               .HasOne(a => a.Product)
               .WithMany(p => p.Assets)
               .HasForeignKey(a => a.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductReview>()
                .HasOne(pr => pr.Order)
                .WithMany(o => o.ProductReviews)
                .HasForeignKey(pr => pr.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

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
        }
    }
}
