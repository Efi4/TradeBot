using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeBot.Data.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AdjustPrimaryKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WeaponPrices",
                table: "WeaponPrices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArmorPrices",
                table: "ArmorPrices");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeaponPrices",
                table: "WeaponPrices",
                columns: new[] { "Type", "Attack", "Crit" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArmorPrices",
                table: "ArmorPrices",
                columns: new[] { "Type", "Stat" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WeaponPrices",
                table: "WeaponPrices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArmorPrices",
                table: "ArmorPrices");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeaponPrices",
                table: "WeaponPrices",
                column: "Type");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArmorPrices",
                table: "ArmorPrices",
                column: "Type");
        }
    }
}
