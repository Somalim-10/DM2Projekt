using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DM2Projekt.Migrations
{
    /// <inheritdoc />
    public partial class addedmigrationforsmartboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Smartboard",
                columns: table => new
                {
                    SmartboardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    Availability = table.Column<bool>(type: "bit", nullable: false)
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
                name: "IX_Smartboard_RoomId",
                table: "Smartboard",
                column: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Smartboard");
        }
    }
}
