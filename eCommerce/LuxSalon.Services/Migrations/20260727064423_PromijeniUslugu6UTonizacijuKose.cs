using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxSalon.Services.Migrations
{
    /// <inheritdoc />
    public partial class PromijeniUslugu6UTonizacijuKose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FrizerUsluge",
                keyColumn: "Id",
                keyValue: 6,
                column: "FrizerId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Obavijesti",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Naslov", "Tekst" },
                values: new object[] { "Novost: tonizacija kose", "Uveli smo tonizaciju kose. Rezervišite termin i isprobajte je uz popust za prve klijente." });

            migrationBuilder.UpdateData(
                table: "Usluge",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Naziv", "Opis", "Tagovi", "UslugaKategorijaId" },
                values: new object[] { "Tonizacija kose", "Osvježavanje i produbljivanje tona boje kose", "kosa,farbanje,tonizacija", 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FrizerUsluge",
                keyColumn: "Id",
                keyValue: 6,
                column: "FrizerId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Obavijesti",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Naslov", "Tekst" },
                values: new object[] { "Novi tretmani njege lica", "Uveli smo nove anti-age tretmane lica. Rezervišite termin i isprobajte ih uz popust za prve klijente." });

            migrationBuilder.UpdateData(
                table: "Usluge",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Naziv", "Opis", "Tagovi", "UslugaKategorijaId" },
                values: new object[] { "Anti-age tretman lica", "Tretman protiv starenja kože", "lice,njega,antiage", 3 });
        }
    }
}
