using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LuxSalon.Services.Migrations
{
    /// <inheritdoc />
    public partial class DodajLuxSalonEntitete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Frizeri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Biografija = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Specijalizacija = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frizeri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Frizeri_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UslugaKategorije",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UslugaKategorije", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usluge",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Cijena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrajanjeMinuta = table.Column<int>(type: "int", nullable: false),
                    UslugaKategorijaId = table.Column<int>(type: "int", nullable: true),
                    Tagovi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usluge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usluge_UslugaKategorije_UslugaKategorijaId",
                        column: x => x.UslugaKategorijaId,
                        principalTable: "UslugaKategorije",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FrizerUsluge",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FrizerId = table.Column<int>(type: "int", nullable: false),
                    UslugaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrizerUsluge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FrizerUsluge_Frizeri_FrizerId",
                        column: x => x.FrizerId,
                        principalTable: "Frizeri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FrizerUsluge_Usluge_UslugaId",
                        column: x => x.UslugaId,
                        principalTable: "Usluge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Termini",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KlijentId = table.Column<int>(type: "int", nullable: false),
                    FrizerId = table.Column<int>(type: "int", nullable: false),
                    UslugaId = table.Column<int>(type: "int", nullable: false),
                    DatumVrijeme = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrajanjeMinuta = table.Column<int>(type: "int", nullable: false),
                    Cijena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Napomena = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Termini", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Termini_Frizeri_FrizerId",
                        column: x => x.FrizerId,
                        principalTable: "Frizeri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Termini_Users_KlijentId",
                        column: x => x.KlijentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Termini_Usluge_UslugaId",
                        column: x => x.UslugaId,
                        principalTable: "Usluge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifikacije",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KorisnikId = table.Column<int>(type: "int", nullable: false),
                    Naslov = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Poruka = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    Procitano = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TerminId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifikacije", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifikacije_Termini_TerminId",
                        column: x => x.TerminId,
                        principalTable: "Termini",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifikacije_Users_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Placanja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TerminId = table.Column<int>(type: "int", nullable: false),
                    Iznos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaypalOrderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaypalTransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatumPlacanja = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DatumPovrata = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Placanja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Placanja_Termini_TerminId",
                        column: x => x.TerminId,
                        principalTable: "Termini",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name" },
                values: new object[] { 3, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Zaposlenik koji izvodi usluge", true, "Frizer" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "LastLoginAt", "LastName", "PasswordHash", "PasswordSalt", "PhoneNumber", "ProfileImageBase64", "Username" },
                values: new object[,]
                {
                    { 6, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), "frizer1@luxsalon.com", "Amina", true, null, "Hairstyle", "uT1mnLhuBI0P/NNG2qpLRhn+2+4=", "t01xsyErjwiNT3KKqgW76g==", null, null, "frizer1" },
                    { 7, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), "frizer2@luxsalon.com", "Damir", true, null, "Stilist", "RJDZItzNyYVtz0clN6Ke4OIVBFw=", "SNnJGci6It1/4Rjv+3ri+Q==", null, null, "frizer2" }
                });

            migrationBuilder.InsertData(
                table: "UslugaKategorije",
                columns: new[] { "Id", "CreatedAt", "IsActive", "Naziv", "Opis" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, "Šišanje", "Šišanje i oblikovanje kose" },
                    { 2, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, "Farbanje", "Bojenje i tretmani boje kose" },
                    { 3, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, "Njega lica", "Tretmani njege i čišćenja lica" },
                    { 4, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, "Njega ruku i nogu", "Manikura i pedikura" }
                });

            migrationBuilder.InsertData(
                table: "Frizeri",
                columns: new[] { "Id", "Biografija", "CreatedAt", "IsActive", "Specijalizacija", "UserId" },
                values: new object[,]
                {
                    { 1, "10 godina iskustva u šišanju i farbanju.", new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, "Kosa - šišanje i farbanje", 6 },
                    { 2, "Specijalista za njegu lica, ruku i nogu.", new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, "Njega lica, manikura, pedikura", 7 }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "DateAssigned", "RoleId", "UserId" },
                values: new object[,]
                {
                    { 6, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), 3, 6 },
                    { 7, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), 3, 7 }
                });

            migrationBuilder.InsertData(
                table: "Usluge",
                columns: new[] { "Id", "Cijena", "CreatedAt", "IsActive", "Naziv", "Opis", "Tagovi", "TrajanjeMinuta", "UpdatedAt", "UslugaKategorijaId" },
                values: new object[,]
                {
                    { 1, 15m, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, "Muško šišanje", "Klasično muško šišanje", "kosa,musko,sisanje", 30, null, 1 },
                    { 2, 25m, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, "Žensko šišanje", "Šišanje i oblikovanje za žene", "kosa,zensko,sisanje", 45, null, 1 },
                    { 3, 60m, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, "Farbanje kose", "Jednobojno farbanje kose", "kosa,farbanje,boja", 90, null, 2 },
                    { 4, 80m, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, "Pramenovi", "Melirani pramenovi", "kosa,farbanje,pramenovi", 120, null, 2 },
                    { 5, 40m, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, "Čišćenje lica", "Dubinsko čišćenje lica", "lice,njega,cisenje", 60, null, 3 },
                    { 6, 55m, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, "Anti-age tretman lica", "Tretman protiv starenja kože", "lice,njega,antiage", 60, null, 3 },
                    { 7, 20m, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, "Manikura", "Klasična manikura", "ruke,manikura,njega", 40, null, 4 },
                    { 8, 25m, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, "Pedikura", "Klasična pedikura", "noge,pedikura,njega", 50, null, 4 }
                });

            migrationBuilder.InsertData(
                table: "FrizerUsluge",
                columns: new[] { "Id", "FrizerId", "UslugaId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 1, 2 },
                    { 3, 1, 3 },
                    { 4, 1, 4 },
                    { 5, 2, 5 },
                    { 6, 2, 6 },
                    { 7, 2, 7 },
                    { 8, 2, 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Frizeri_UserId",
                table: "Frizeri",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FrizerUsluge_FrizerId",
                table: "FrizerUsluge",
                column: "FrizerId");

            migrationBuilder.CreateIndex(
                name: "IX_FrizerUsluge_UslugaId",
                table: "FrizerUsluge",
                column: "UslugaId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifikacije_KorisnikId",
                table: "Notifikacije",
                column: "KorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifikacije_TerminId",
                table: "Notifikacije",
                column: "TerminId");

            migrationBuilder.CreateIndex(
                name: "IX_Placanja_TerminId",
                table: "Placanja",
                column: "TerminId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Termini_FrizerId",
                table: "Termini",
                column: "FrizerId");

            migrationBuilder.CreateIndex(
                name: "IX_Termini_KlijentId",
                table: "Termini",
                column: "KlijentId");

            migrationBuilder.CreateIndex(
                name: "IX_Termini_UslugaId",
                table: "Termini",
                column: "UslugaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usluge_UslugaKategorijaId",
                table: "Usluge",
                column: "UslugaKategorijaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FrizerUsluge");

            migrationBuilder.DropTable(
                name: "Notifikacije");

            migrationBuilder.DropTable(
                name: "Placanja");

            migrationBuilder.DropTable(
                name: "Termini");

            migrationBuilder.DropTable(
                name: "Frizeri");

            migrationBuilder.DropTable(
                name: "Usluge");

            migrationBuilder.DropTable(
                name: "UslugaKategorije");

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
