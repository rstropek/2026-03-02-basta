using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChatBotDb.Migrations
{
    /// <inheritdoc />
    public partial class AddBouquetPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BouquetPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Size = table.Column<string>(type: "TEXT", nullable: false),
                    NumberOfFlowers = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BouquetPrices", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "BouquetPrices",
                columns: new[] { "Id", "Description", "NumberOfFlowers", "Price", "Size" },
                values: new object[,]
                {
                    { 1, "3 flowers arranged with a little bit of green grass", 3, 15m, "Small" },
                    { 2, "5 flowers nicely arranged, including some larger green leaves as decoration", 5, 25m, "Medium" },
                    { 3, "10 flowers, beautifully arranged with greenery and smaller filler flowers", 10, 35m, "Large" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BouquetPrices");
        }
    }
}
