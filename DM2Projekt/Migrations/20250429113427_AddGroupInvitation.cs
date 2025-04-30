using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DM2Projekt.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Group",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GroupInvitation",
                columns: table => new
                {
                    InvitationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    InvitedUserId = table.Column<int>(type: "int", nullable: false),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupInvitation", x => x.InvitationId);
                    table.ForeignKey(
                        name: "FK_GroupInvitation_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupInvitation_User_InvitedUserId",
                        column: x => x.InvitedUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Group_CreatedByUserId",
                table: "Group",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvitation_GroupId",
                table: "GroupInvitation",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvitation_InvitedUserId",
                table: "GroupInvitation",
                column: "InvitedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Group_User_CreatedByUserId",
                table: "Group",
                column: "CreatedByUserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Group_User_CreatedByUserId",
                table: "Group");

            migrationBuilder.DropTable(
                name: "GroupInvitation");

            migrationBuilder.DropIndex(
                name: "IX_Group_CreatedByUserId",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Group");
        }
    }
}
