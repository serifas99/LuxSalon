using Microsoft.EntityFrameworkCore;

namespace LuxSalon.Services.Database
{
    public partial class ECommerceDbContext : DbContext
    {
        private void CreateSeed(ModelBuilder modelBuilder)
        {
            SeedRoles(modelBuilder);
            SeedUsers(modelBuilder);
            SeedUserRoles(modelBuilder);

            SeedLuxSalon(modelBuilder);
        }

        private void SeedLuxSalon(ModelBuilder modelBuilder)
        {
            // Dodatna rola za frizere (Admin = 1, Customer = 2 vec postoje)
            modelBuilder.Entity<Role>().HasData(
                new
                {
                    Id = 3,
                    Name = "Frizer",
                    Description = "Zaposlenik koji izvodi usluge",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Korisnicki nalozi za frizere (lozinka za oba: Test123!)
            modelBuilder.Entity<User>().HasData(
                new
                {
                    Id = 6,
                    FirstName = "Amina",
                    LastName = "Hairstyle",
                    Email = "frizer1@luxsalon.com",
                    Username = "frizer1",
                    PasswordHash = "IV/0YG2BWZ+smbLeXBpH+ZbyMLU=", // Test123!
                    PasswordSalt = "nxuCJ53rAjnOZO8Dh/rRoQ==",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc),
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null
                },
                new
                {
                    Id = 7,
                    FirstName = "Damir",
                    LastName = "Stilist",
                    Email = "frizer2@luxsalon.com",
                    Username = "frizer2",
                    PasswordHash = "eJPc308v8Kl8xYjCa5IYnV5g2Dw=", // Test123!
                    PasswordSalt = "s+SJ0wp/uSRl29HAF6L2yw==",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc),
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null
                }
            );

            modelBuilder.Entity<UserRole>().HasData(
                new
                {
                    Id = 6,
                    UserId = 6,
                    RoleId = 3,
                    DateAssigned = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc)
                },
                new
                {
                    Id = 7,
                    UserId = 7,
                    RoleId = 3,
                    DateAssigned = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Kategorije usluga
            modelBuilder.Entity<UslugaKategorija>().HasData(
                new { Id = 1, Naziv = "Šišanje", Opis = "Šišanje i oblikovanje kose", IsActive = true, CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = 2, Naziv = "Farbanje", Opis = "Bojenje i tretmani boje kose", IsActive = true, CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = 3, Naziv = "Njega lica", Opis = "Tretmani njege i čišćenja lica", IsActive = true, CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = 4, Naziv = "Njega ruku i nogu", Opis = "Manikura i pedikura", IsActive = true, CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc) }
            );

            // Usluge
            modelBuilder.Entity<Usluga>().HasData(
                new { Id = 1, Naziv = "Muško šišanje", Opis = "Klasično muško šišanje", Cijena = 15m, TrajanjeMinuta = 30, UslugaKategorijaId = (int?)1, Tagovi = "kosa,musko,sisanje", IsActive = true, CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
                new { Id = 2, Naziv = "Žensko šišanje", Opis = "Šišanje i oblikovanje za žene", Cijena = 25m, TrajanjeMinuta = 45, UslugaKategorijaId = (int?)1, Tagovi = "kosa,zensko,sisanje", IsActive = true, CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
                new { Id = 3, Naziv = "Farbanje kose", Opis = "Jednobojno farbanje kose", Cijena = 60m, TrajanjeMinuta = 90, UslugaKategorijaId = (int?)2, Tagovi = "kosa,farbanje,boja", IsActive = true, CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
                new { Id = 4, Naziv = "Pramenovi", Opis = "Melirani pramenovi", Cijena = 80m, TrajanjeMinuta = 120, UslugaKategorijaId = (int?)2, Tagovi = "kosa,farbanje,pramenovi", IsActive = true, CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
                new { Id = 5, Naziv = "Čišćenje lica", Opis = "Dubinsko čišćenje lica", Cijena = 40m, TrajanjeMinuta = 60, UslugaKategorijaId = (int?)3, Tagovi = "lice,njega,cisenje", IsActive = true, CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
                new { Id = 6, Naziv = "Tonizacija kose", Opis = "Osvježavanje i produbljivanje tona boje kose", Cijena = 55m, TrajanjeMinuta = 60, UslugaKategorijaId = (int?)2, Tagovi = "kosa,farbanje,tonizacija", IsActive = true, CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
                new { Id = 7, Naziv = "Manikura", Opis = "Klasična manikura", Cijena = 20m, TrajanjeMinuta = 40, UslugaKategorijaId = (int?)4, Tagovi = "ruke,manikura,njega", IsActive = true, CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
                new { Id = 8, Naziv = "Pedikura", Opis = "Klasična pedikura", Cijena = 25m, TrajanjeMinuta = 50, UslugaKategorijaId = (int?)4, Tagovi = "noge,pedikura,njega", IsActive = true, CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null }
            );

            // Frizeri
            modelBuilder.Entity<Frizer>().HasData(
                new { Id = 1, UserId = 6, Biografija = "10 godina iskustva u šišanju i farbanju.", Specijalizacija = "Kosa - šišanje i farbanje", IsActive = true, CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = 2, UserId = 7, Biografija = "Specijalista za njegu lica, ruku i nogu.", Specijalizacija = "Njega lica, manikura, pedikura", IsActive = true, CreatedAt = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc) }
            );

            // Koje usluge koji frizer izvodi
            modelBuilder.Entity<FrizerUsluga>().HasData(
                new { Id = 1, FrizerId = 1, UslugaId = 1 },
                new { Id = 2, FrizerId = 1, UslugaId = 2 },
                new { Id = 3, FrizerId = 1, UslugaId = 3 },
                new { Id = 4, FrizerId = 1, UslugaId = 4 },
                new { Id = 5, FrizerId = 2, UslugaId = 5 },
                new { Id = 6, FrizerId = 1, UslugaId = 6 },
                new { Id = 7, FrizerId = 2, UslugaId = 7 },
                new { Id = 8, FrizerId = 2, UslugaId = 8 }
            );

            // Radno vrijeme oba frizera: Pon-Pet 08-17h, Subota 09-15h, Nedjelja ne radi
            var radniDani = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
            var radnoVrijemeSeed = new List<object>();
            int rvId = 1;
            foreach (var frizerId in new[] { 1, 2 })
            {
                foreach (var dan in radniDani)
                {
                    radnoVrijemeSeed.Add(new { Id = rvId++, FrizerId = frizerId, DanUSedmici = dan, PocetakRada = new TimeSpan(8, 0, 0), KrajRada = new TimeSpan(17, 0, 0), NeRadi = false });
                }
                radnoVrijemeSeed.Add(new { Id = rvId++, FrizerId = frizerId, DanUSedmici = DayOfWeek.Saturday, PocetakRada = new TimeSpan(9, 0, 0), KrajRada = new TimeSpan(15, 0, 0), NeRadi = false });
                radnoVrijemeSeed.Add(new { Id = rvId++, FrizerId = frizerId, DanUSedmici = DayOfWeek.Sunday, PocetakRada = new TimeSpan(0, 0, 0), KrajRada = new TimeSpan(0, 0, 0), NeRadi = true });
            }
            modelBuilder.Entity<RadnoVrijeme>().HasData(radnoVrijemeSeed.ToArray());

            // Salonske obavijesti (news) - odvojeno od licnih notifikacija
            modelBuilder.Entity<Obavijest>().HasData(
                new { Id = 1, Naslov = "Dobrodošli u LuxSalon!", Tekst = "Sada možete zakazivati termine direktno kroz aplikaciju, pratiti svoje rezervacije i primati obavještenja uživo.", SlikaBase64 = (string?)null, IsActive = true, CreatedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = 2, Naslov = "Novost: tonizacija kose", Tekst = "Uveli smo tonizaciju kose. Rezervišite termin i isprobajte je uz popust za prve klijente.", SlikaBase64 = (string?)null, IsActive = true, CreatedAt = new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = 3, Naslov = "Radno vrijeme za praznike", Tekst = "Za vrijeme predstojećih praznika salon radi po skraćenom radnom vremenu. Provjerite dostupne termine u aplikaciji.", SlikaBase64 = (string?)null, IsActive = true, CreatedAt = new DateTime(2026, 7, 18, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
        private void SeedRoles(ModelBuilder modelBuilder)
        {
            // Seed Roles - deterministic Ids: 1 = Admin, 2 = Customer
            modelBuilder.Entity<Role>().HasData(
                new
                {
                    Id = 1,
                    Name = "Admin",
                    Description = "Administrator role with full permissions",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc)
                },
                new
                {
                    Id = 2,
                    Name = "Customer",
                    Description = "Default customer role",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }

        private void SeedUsers(ModelBuilder modelBuilder)
        {
            // Seed Users - 3 admins (Ids 1-3) and 2 customers (Ids 4-5)
            modelBuilder.Entity<User>().HasData(
                new
                {
                    Id = 1,
                    FirstName = "Alice",
                    LastName = "Admin",
                    Email = "admin1@gmail.com",
                    Username = "admin1",
                    PasswordHash = "5kRBQg4Ufcx4hAknG7P9zhfLPvY=", // Test123
                    PasswordSalt = "FmvmUwPsJyRRffhNRQvbrA==",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc),
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null
                },
                new
                {
                    Id = 2,
                    FirstName = "Bob",
                    LastName = "Admin",
                    Email = "admin2@gmail.com",
                    Username = "admin2",
                    PasswordHash = "GBoyh1WP+OMgGjqRj6vK6L1+oGc=", // Test123
                    PasswordSalt = "0AXpKx6xRp9xM42jCf/PiA==",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc),
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null
                },
                new
                {
                    Id = 3,
                    FirstName = "Carol",
                    LastName = "Admin",
                    Email = "admin3@gmail.com",
                    Username = "admin3",
                    PasswordHash = "x6JHKCTQywdAzTcZxGWFvrKPORM=", // Test123
                    PasswordSalt = "IwhTfKQNgyqWfOlTqCDXrg==",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc),
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null
                },
                new
                {
                    Id = 4,
                    FirstName = "Dave",
                    LastName = "Customer",
                    Email = "customer1@gmail.com",
                    Username = "customer1",
                    PasswordHash = "E0fA2/f9GZvIRRt/cgqQemG/Cog=", // Test123
                    PasswordSalt = "TiJxWTJcd7sBSiWNbhK9Vw==",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc),
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null
                },
                new
                {
                    Id = 5,
                    FirstName = "Eve",
                    LastName = "Customer",
                    Email = "customer2@gmail.com",
                    Username = "customer2",
                    PasswordHash = "Ov4LxpWKXXV9dwMYvBgqODdzIt0=", // Test123
                    PasswordSalt = "KtWF6g7SemBqs4nVWV4Ziw==",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc),
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null
                }
            );
        }

        private void SeedUserRoles(ModelBuilder modelBuilder)
        {
            // Map users to roles (UserRole has its own Id PK)
            // Admin role = RoleId 1, Customer role = RoleId 2
            modelBuilder.Entity<UserRole>().HasData(
                new
                {
                    Id = 1,
                    UserId = 1,
                    RoleId = 1,
                    DateAssigned = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc)
                },
                new
                {
                    Id = 2,
                    UserId = 2,
                    RoleId = 1,
                    DateAssigned = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc)
                },
                new
                {
                    Id = 3,
                    UserId = 3,
                    RoleId = 1,
                    DateAssigned = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc)
                },
                new
                {
                    Id = 4,
                    UserId = 4,
                    RoleId = 2,
                    DateAssigned = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc)
                },
                new
                {
                    Id = 5,
                    UserId = 5,
                    RoleId = 2,
                    DateAssigned = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
