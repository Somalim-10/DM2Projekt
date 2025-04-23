using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DM2Projekt.Migrations
{
    /// <inheritdoc />
    public partial class ChangedNameForCreatedByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_User_CreatedByUserUserId",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "CreateByUserId",
                table: "Booking");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserUserId",
                table: "Booking",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Booking_CreatedByUserUserId",
                table: "Booking",
                newName: "IX_Booking_CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_User_CreatedByUserId",
                table: "Booking",
                column: "CreatedByUserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_User_CreatedByUserId",
                table: "Booking");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Booking",
                newName: "CreatedByUserUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Booking_CreatedByUserId",
                table: "Booking",
                newName: "IX_Booking_CreatedByUserUserId");

            migrationBuilder.AddColumn<int>(
                name: "CreateByUserId",
                table: "Booking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_User_CreatedByUserUserId",
                table: "Booking",
                column: "CreatedByUserUserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
