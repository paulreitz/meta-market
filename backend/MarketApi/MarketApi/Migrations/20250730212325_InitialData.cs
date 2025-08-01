using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    WalletAddress = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PfpUrl = table.Column<string>(type: "text", nullable: false),
                    BannerUrl = table.Column<string>(type: "text", nullable: false),
                    Bio = table.Column<string>(type: "text", nullable: false),
                    Website = table.Column<string>(type: "text", nullable: false),
                    AdultContentEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Joined = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Nonce = table.Column<string>(type: "text", nullable: true),
                    NonceExpiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.WalletAddress);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
