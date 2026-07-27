using Microsoft.EntityFrameworkCore;

namespace LuxSalon.Services.Database
{
    public partial class ECommerceDbContext : DbContext
    {
        public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }

        // LuxSalon domain
        public DbSet<UslugaKategorija> UslugaKategorije { get; set; }
        public DbSet<Usluga> Usluge { get; set; }
        public DbSet<Frizer> Frizeri { get; set; }
        public DbSet<FrizerUsluga> FrizerUsluge { get; set; }
        public DbSet<Termin> Termini { get; set; }
        public DbSet<Placanje> Placanja { get; set; }
        public DbSet<Notifikacija> Notifikacije { get; set; }
        public DbSet<FrizerOcjena> FrizerOcjene { get; set; }
        public DbSet<RadnoVrijeme> RadnaVremena { get; set; }
        public DbSet<IstorijaStatusaTermina> IstorijaStatusaTermina { get; set; }
        public DbSet<Obavijest> Obavijesti { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            CreateConfiguration(modelBuilder);

            CreateSeed(modelBuilder);
            
        }

       
    }
}
