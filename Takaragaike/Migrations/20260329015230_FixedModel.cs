using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Takaragaike.Migrations
{
    /// <inheritdoc />
    public partial class FixedModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Algorithm",
                table: "OtpData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Digits",
                table: "OtpData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Algorithm",
                table: "OtpData");

            migrationBuilder.DropColumn(
                name: "Digits",
                table: "OtpData");
        }
    }
}
