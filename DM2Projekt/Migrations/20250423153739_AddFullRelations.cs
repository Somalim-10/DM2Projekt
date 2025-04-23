using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DM2Projekt.Migrations
{
    /// <inheritdoc />
    public partial class AddFullRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Group_GroupId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Room_RoomId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Smartboard_SmartboardId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_User_CreatedByUserId",
                table: "Booking");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Group_GroupId",
                table: "Booking",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Room_RoomId",
                table: "Booking",
                column: "RoomId",
                principalTable: "Room",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Smartboard_SmartboardId",
                table: "Booking",
                column: "SmartboardId",
                principalTable: "Smartboard",
                principalColumn: "SmartboardId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_User_CreatedByUserId",
                table: "Booking",
                column: "CreatedByUserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Group_GroupId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Room_RoomId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Smartboard_SmartboardId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_User_CreatedByUserId",
                table: "Booking");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Group_GroupId",
                table: "Booking",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Room_RoomId",
                table: "Booking",
                column: "RoomId",
                principalTable: "Room",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Smartboard_SmartboardId",
                table: "Booking",
                column: "SmartboardId",
                principalTable: "Smartboard",
                principalColumn: "SmartboardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_User_CreatedByUserId",
                table: "Booking",
                column: "CreatedByUserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
