using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DM2Projekt.Migrations
{
    /// <inheritdoc />
    public partial class MakeRoomCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Room_RoomId1",
                table: "Booking");

            migrationBuilder.DropIndex(
                name: "IX_Booking_RoomId1",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "RoomId1",
                table: "Booking");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoomId1",
                table: "Booking",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Booking_RoomId1",
                table: "Booking",
                column: "RoomId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Room_RoomId1",
                table: "Booking",
                column: "RoomId1",
                principalTable: "Room",
                principalColumn: "RoomId");
        }
    }
}
