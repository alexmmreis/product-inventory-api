using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "100000, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CreatedAtUtc", "Description", "Name", "Price", "Stock", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 100001, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "2.4GHz wireless mouse with USB receiver", "Wireless Mouse", 19.99m, 150, null },
                    { 100002, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tenkeyless mechanical keyboard, blue switches", "Mechanical Keyboard", 79.99m, 75, null },
                    { 100003, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "27-inch QHD IPS monitor, 144Hz", "27-inch Monitor", 299.00m, 30, null },
                    { 100004, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "7-in-1 USB-C hub with HDMI and card reader", "USB-C Hub", 34.50m, 200, null },
                    { 100005, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Full HD webcam with built-in microphone", "Webcam 1080p", 45.00m, 0, null },
                    { 100006, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aluminum adjustable laptop stand", "Laptop Stand", 29.99m, 120, null },
                    { 100007, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Over-ear ANC headphones, 30h battery", "Noise Cancelling Headphones", 149.99m, 60, null },
                    { 100008, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Portable USB 3.2 SSD, 1TB", "External SSD 1TB", 89.99m, 90, null },
                    { 100009, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Extended desk mat, 900x400mm", "Desk Mat", 15.00m, 5, null },
                    { 100010, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mesh-back ergonomic office chair", "Ergonomic Chair", 219.00m, 12, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
