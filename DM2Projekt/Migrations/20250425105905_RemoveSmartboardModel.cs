using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DM2Projekt.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSmartboardModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Smartboard_SmartboardId",
                table: "Booking");

            migrationBuilder.DropTable(
                name: "Smartboard");

            migrationBuilder.DropIndex(
                name: "IX_Booking_SmartboardId",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "SmartboardId",
                table: "Booking");

            migrationBuilder.AddColumn<bool>(
                name: "UsesSmartboard",
                table: "Booking",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsesSmartboard",
                table: "Booking");

            migrationBuilder.AddColumn<int>(
                name: "SmartboardId",
                table: "Booking",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Smartboard",
                columns: table => new
                {
                    SmartboardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Smartboard", x => x.SmartboardId);
                    table.ForeignKey(
                        name: "FK_Smartboard_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_SmartboardId",
                table: "Booking",
                column: "SmartboardId");

            migrationBuilder.CreateIndex(
                name: "IX_Smartboard_RoomId",
                table: "Smartboard",
                column: "RoomId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Smartboard_SmartboardId",
                table: "Booking",
                column: "SmartboardId",
                principalTable: "Smartboard",
                principalColumn: "SmartboardId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
