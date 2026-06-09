using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zapas.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Sessions",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_OwnerUserId",
                table: "Sessions",
                column: "OwnerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_OwnerUserId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Sessions");
        }
    }
}
