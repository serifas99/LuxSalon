using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LuxSalon.Services.Migrations
{
    /// <inheritdoc />
    public partial class DodajOcjeneRadnoVrijemeAuditObavijesti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "ProductReviews");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "ProductTypes");

            migrationBuilder.DropTable(
                name: "UnitOfMeasures");

            migrationBuilder.CreateTable(
                name: "FrizerOcjene",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TerminId = table.Column<int>(type: "int", nullable: false),
                    KlijentId = table.Column<int>(type: "int", nullable: false),
                    FrizerId = table.Column<int>(type: "int", nullable: false),
                    Ocjena = table.Column<int>(type: "int", nullable: false),
                    Komentar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrizerOcjene", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FrizerOcjene_Frizeri_FrizerId",
                        column: x => x.FrizerId,
                        principalTable: "Frizeri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FrizerOcjene_Termini_TerminId",
                        column: x => x.TerminId,
                        principalTable: "Termini",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FrizerOcjene_Users_KlijentId",
                        column: x => x.KlijentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IstorijaStatusaTermina",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TerminId = table.Column<int>(type: "int", nullable: false),
                    PrethodniStatus = table.Column<int>(type: "int", nullable: false),
                    NoviStatus = table.Column<int>(type: "int", nullable: false),
                    PromijenioKorisnikId = table.Column<int>(type: "int", nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IstorijaStatusaTermina", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IstorijaStatusaTermina_Termini_TerminId",
                        column: x => x.TerminId,
                        principalTable: "Termini",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IstorijaStatusaTermina_Users_PromijenioKorisnikId",
                        column: x => x.PromijenioKorisnikId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Obavijesti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naslov = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Tekst = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SlikaBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Obavijesti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RadnaVremena",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FrizerId = table.Column<int>(type: "int", nullable: false),
                    DanUSedmici = table.Column<int>(type: "int", nullable: false),
                    PocetakRada = table.Column<TimeSpan>(type: "time", nullable: false),
                    KrajRada = table.Column<TimeSpan>(type: "time", nullable: false),
                    NeRadi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadnaVremena", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RadnaVremena_Frizeri_FrizerId",
                        column: x => x.FrizerId,
                        principalTable: "Frizeri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Obavijesti",
                columns: new[] { "Id", "CreatedAt", "IsActive", "Naslov", "SlikaBase64", "Tekst" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Dobrodošli u LuxSalon!", null, "Sada možete zakazivati termine direktno kroz aplikaciju, pratiti svoje rezervacije i primati obavještenja uživo." },
                    { 2, new DateTime(2026, 7, 10, 0, 0, 0, 0, DateTimeKind.Utc), true, "Novi tretmani njege lica", null, "Uveli smo nove anti-age tretmane lica. Rezervišite termin i isprobajte ih uz popust za prve klijente." },
                    { 3, new DateTime(2026, 7, 18, 0, 0, 0, 0, DateTimeKind.Utc), true, "Radno vrijeme za praznike", null, "Za vrijeme predstojećih praznika salon radi po skraćenom radnom vremenu. Provjerite dostupne termine u aplikaciji." }
                });

            migrationBuilder.InsertData(
                table: "RadnaVremena",
                columns: new[] { "Id", "DanUSedmici", "FrizerId", "KrajRada", "NeRadi", "PocetakRada" },
                values: new object[,]
                {
                    { 1, 1, 1, new TimeSpan(0, 17, 0, 0, 0), false, new TimeSpan(0, 8, 0, 0, 0) },
                    { 2, 2, 1, new TimeSpan(0, 17, 0, 0, 0), false, new TimeSpan(0, 8, 0, 0, 0) },
                    { 3, 3, 1, new TimeSpan(0, 17, 0, 0, 0), false, new TimeSpan(0, 8, 0, 0, 0) },
                    { 4, 4, 1, new TimeSpan(0, 17, 0, 0, 0), false, new TimeSpan(0, 8, 0, 0, 0) },
                    { 5, 5, 1, new TimeSpan(0, 17, 0, 0, 0), false, new TimeSpan(0, 8, 0, 0, 0) },
                    { 6, 6, 1, new TimeSpan(0, 15, 0, 0, 0), false, new TimeSpan(0, 9, 0, 0, 0) },
                    { 7, 0, 1, new TimeSpan(0, 0, 0, 0, 0), true, new TimeSpan(0, 0, 0, 0, 0) },
                    { 8, 1, 2, new TimeSpan(0, 17, 0, 0, 0), false, new TimeSpan(0, 8, 0, 0, 0) },
                    { 9, 2, 2, new TimeSpan(0, 17, 0, 0, 0), false, new TimeSpan(0, 8, 0, 0, 0) },
                    { 10, 3, 2, new TimeSpan(0, 17, 0, 0, 0), false, new TimeSpan(0, 8, 0, 0, 0) },
                    { 11, 4, 2, new TimeSpan(0, 17, 0, 0, 0), false, new TimeSpan(0, 8, 0, 0, 0) },
                    { 12, 5, 2, new TimeSpan(0, 17, 0, 0, 0), false, new TimeSpan(0, 8, 0, 0, 0) },
                    { 13, 6, 2, new TimeSpan(0, 15, 0, 0, 0), false, new TimeSpan(0, 9, 0, 0, 0) },
                    { 14, 0, 2, new TimeSpan(0, 0, 0, 0, 0), true, new TimeSpan(0, 0, 0, 0, 0) }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "IV/0YG2BWZ+smbLeXBpH+ZbyMLU=", "nxuCJ53rAjnOZO8Dh/rRoQ==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "eJPc308v8Kl8xYjCa5IYnV5g2Dw=", "s+SJ0wp/uSRl29HAF6L2yw==" });

            migrationBuilder.CreateIndex(
                name: "IX_FrizerOcjene_FrizerId",
                table: "FrizerOcjene",
                column: "FrizerId");

            migrationBuilder.CreateIndex(
                name: "IX_FrizerOcjene_KlijentId",
                table: "FrizerOcjene",
                column: "KlijentId");

            migrationBuilder.CreateIndex(
                name: "IX_FrizerOcjene_TerminId",
                table: "FrizerOcjene",
                column: "TerminId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IstorijaStatusaTermina_PromijenioKorisnikId",
                table: "IstorijaStatusaTermina",
                column: "PromijenioKorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_IstorijaStatusaTermina_TerminId",
                table: "IstorijaStatusaTermina",
                column: "TerminId");

            migrationBuilder.CreateIndex(
                name: "IX_RadnaVremena_FrizerId_DanUSedmici",
                table: "RadnaVremena",
                columns: new[] { "FrizerId", "DanUSedmici" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FrizerOcjene");

            migrationBuilder.DropTable(
                name: "IstorijaStatusaTermina");

            migrationBuilder.DropTable(
                name: "Obavijesti");

            migrationBuilder.DropTable(
                name: "RadnaVremena");

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentTransactionId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShippingAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShippingCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShippingCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShippingState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShippingZipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitOfMeasures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Abbreviation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductTypeId = table.Column<int>(type: "int", nullable: true),
                    UnitOfMeasureId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductState = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_ProductTypes_ProductTypeId",
                        column: x => x.ProductTypeId,
                        principalTable: "ProductTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_UnitOfMeasures_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Base64Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assets_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCategories_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductReviews_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductReviews_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductReviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "ParentCategoryId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Electronic devices and accessories", true, "Electronics", null, null },
                    { 4, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Device accessories and peripherals", true, "Accessories", null, null }
                });

            migrationBuilder.InsertData(
                table: "ProductTypes",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Tangible products that require shipping", true, "Physical", null },
                    { 2, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Intangible products that can be downloaded", true, "Digital", null },
                    { 3, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Non-physical products that provide a service", true, "Service", null }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "Price", "ProductState", "ProductTypeId", "SKU", "StockQuantity", "UnitOfMeasureId", "UpdatedAt", "Weight" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "High-performance laptop suitable for gaming and development", true, "Gaming Laptop", 999.99m, "DraftProductState", null, "LAP-1000", 10, null, null, 2500m },
                    { 2, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Latest generation smartphone with advanced camera features", true, "Smartphone X", 699.99m, "DraftProductState", null, "PHN-2000", 25, null, null, 180m },
                    { 3, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Ergonomic wireless mouse with long battery life", true, "Wireless Mouse", 19.99m, "DraftProductState", null, "MSE-300", 150, null, null, 100m },
                    { 4, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "65W USB-C fast charger compatible with laptops and phones", true, "USB-C Fast Charger", 29.99m, "DraftProductState", null, "CHR-400", 200, null, null, 120m },
                    { 5, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "RGB mechanical keyboard with tactile switches", true, "Mechanical Keyboard", 89.99m, "DraftProductState", null, "KEY-500", 75, null, null, 900m },
                    { 6, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Over-ear headphones with active noise cancellation", true, "Noise-Cancelling Headphones", 199.99m, "DraftProductState", null, "HDP-600", 40, null, null, 350m },
                    { 7, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "27-inch 4K UHD monitor with HDR and low response time", true, "27\" 4K Monitor", 349.99m, "DraftProductState", null, "MON-700", 30, null, null, 4500m },
                    { 8, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Portable 1TB external SSD with high-speed USB-C connectivity", true, "External SSD 1TB", 129.99m, "DraftProductState", null, "SSD-800", 60, null, null, 80m },
                    { 9, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Full HD webcam with built-in microphone and privacy shutter", true, "Webcam Pro 1080p", 59.99m, "DraftProductState", null, "CAM-900", 90, null, null, 140m },
                    { 10, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Compact Bluetooth speaker with waterproof design and deep bass", true, "Bluetooth Speaker", 49.99m, "DraftProductState", null, "SPK-1000", 110, null, null, 620m },
                    { 11, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Fitness-focused smartwatch with heart-rate tracking and GPS", true, "Smartwatch Active", 149.99m, "DraftProductState", null, "WCH-1100", 55, null, null, 50m },
                    { 12, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Adjustable aluminum laptop stand for improved desk ergonomics", true, "Laptop Stand Aluminum", 39.99m, "DraftProductState", null, "STD-1200", 85, null, null, 750m },
                    { 13, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Dual-band Wi-Fi 6 router with extended coverage for home networks", true, "Wi-Fi 6 Router", 119.99m, "DraftProductState", null, "RTR-1300", 45, null, null, 680m },
                    { 14, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Digital drawing tablet with pressure-sensitive stylus", true, "Graphics Tablet", 79.99m, "DraftProductState", null, "TAB-1400", 35, null, null, 420m },
                    { 15, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "20000mAh portable power bank with dual USB output", true, "Portable Power Bank", 34.99m, "DraftProductState", null, "PWR-1500", 130, null, null, 410m },
                    { 16, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Cat 6 Ethernet cable for reliable high-speed wired networking", true, "Ethernet Cable 10m", 12.99m, "DraftProductState", null, "NET-1600", 300, null, null, 260m },
                    { 17, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "USB-C docking station with HDMI, Ethernet, USB-A, and card reader ports", true, "Docking Station", 99.99m, "DraftProductState", null, "DOC-1700", 50, null, null, 520m },
                    { 18, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Adjustable LED desk lamp with touch controls and multiple brightness levels", true, "Smart LED Desk Lamp", 44.99m, "DraftProductState", null, "LMP-1800", 70, null, null, 850m },
                    { 19, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Compact 4K action camera with waterproof casing and image stabilization", true, "Action Camera", 179.99m, "DraftProductState", null, "ACT-1900", 28, null, null, 160m },
                    { 20, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Standalone virtual reality headset with motion controllers", true, "VR Headset", 299.99m, "DraftProductState", null, "VRH-2000", 20, null, null, 620m },
                    { 21, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Central smart home hub for connecting lights, sensors, and voice assistants", true, "Smart Home Hub", 84.99m, "DraftProductState", null, "HUB-2100", 42, null, null, 300m },
                    { 22, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Lightweight fitness tracker with step counting, sleep monitoring, and notifications", true, "Fitness Tracker Band", 69.99m, "DraftProductState", null, "FIT-2200", 95, null, null, 35m },
                    { 23, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Slim wireless charging pad compatible with Qi-enabled smartphones and earbuds", true, "Wireless Charging Pad", 24.99m, "DraftProductState", null, "WCP-2300", 160, null, null, 110m },
                    { 24, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Two-bay network attached storage enclosure for backups and media sharing", true, "NAS Storage Enclosure", 229.99m, "DraftProductState", null, "NAS-2400", 18, null, null, 1300m },
                    { 25, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Portable digital voice recorder with noise reduction and long recording time", true, "Digital Voice Recorder", 54.99m, "DraftProductState", null, "REC-2500", 65, null, null, 90m },
                    { 26, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Portable mini projector with HDMI input and built-in speaker", true, "Mini Projector", 159.99m, "DraftProductState", null, "PRJ-2600", 32, null, null, 950m },
                    { 27, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Wi-Fi smart doorbell camera with motion detection and two-way audio", true, "Smart Doorbell Camera", 139.99m, "DraftProductState", null, "DRB-2700", 38, null, null, 250m }
                });

            migrationBuilder.InsertData(
                table: "UnitOfMeasures",
                columns: new[] { "Id", "Abbreviation", "CreatedAt", "Description", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "pc", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "", true, "Piece", null },
                    { 2, "kg", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "", true, "Kilogram", null },
                    { 3, "L", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "", true, "Liter", null }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "uT1mnLhuBI0P/NNG2qpLRhn+2+4=", "t01xsyErjwiNT3KKqgW76g==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "RJDZItzNyYVtz0clN6Ke4OIVBFw=", "SNnJGci6It1/4Rjv+3ri+Q==" });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "ParentCategoryId", "UpdatedAt" },
                values: new object[,]
                {
                    { 2, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Desktops, laptops and related hardware", true, "Computers", 1, null },
                    { 3, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Smartphones and mobile devices", true, "Mobile Phones", 1, null }
                });

            migrationBuilder.InsertData(
                table: "ProductCategories",
                columns: new[] { "Id", "CategoryId", "ProductId" },
                values: new object[,]
                {
                    { 3, 4, 3 },
                    { 4, 4, 4 },
                    { 6, 1, 6 },
                    { 1, 2, 1 },
                    { 2, 3, 2 },
                    { 5, 2, 5 },
                    { 7, 2, 7 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ProductId",
                table: "Assets",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CategoryId",
                table: "ProductCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ProductId",
                table: "ProductCategories",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_OrderId",
                table: "ProductReviews",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductId",
                table: "ProductReviews",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_UserId",
                table: "ProductReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductTypeId",
                table: "Products",
                column: "ProductTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitOfMeasureId",
                table: "Products",
                column: "UnitOfMeasureId");
        }
    }
}
