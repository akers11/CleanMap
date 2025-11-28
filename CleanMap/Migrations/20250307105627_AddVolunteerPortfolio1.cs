using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanMap.Migrations
{
    /// <inheritdoc />
    public partial class AddVolunteerPortfolio1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VolunteerBooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VolunteerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerBooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerAnnouncements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VolunteerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnnouncementId = table.Column<int>(type: "int", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VolunteerBookId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerAnnouncements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolunteerAnnouncements_VolunteerBooks_VolunteerBookId",
                        column: x => x.VolunteerBookId,
                        principalTable: "VolunteerBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerAnnouncements_VolunteerBookId",
                table: "VolunteerAnnouncements",
                column: "VolunteerBookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VolunteerAnnouncements");

            migrationBuilder.DropTable(
                name: "VolunteerBooks");
        }
    }
}
