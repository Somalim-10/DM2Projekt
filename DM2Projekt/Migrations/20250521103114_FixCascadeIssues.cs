using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DM2Projekt.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Group_GroupId",
                table: "Booking");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Group_GroupId",
                table: "Booking",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Group_GroupId",
                table: "Booking");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Group_GroupId",
                table: "Booking",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
