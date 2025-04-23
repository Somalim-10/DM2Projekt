using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DM2Projekt.Migrations
{
    /// <inheritdoc />
    public partial class TestIfFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Smartboard_RoomId",
                table: "Smartboard");

            migrationBuilder.CreateIndex(
                name: "IX_Smartboard_RoomId",
                table: "Smartboard",
                column: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Smartboard_RoomId",
                table: "Smartboard");

            migrationBuilder.CreateIndex(
                name: "IX_Smartboard_RoomId",
                table: "Smartboard",
                column: "RoomId",
                unique: true);
        }
    }
}
