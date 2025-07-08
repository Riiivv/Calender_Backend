using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calender.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDelete_CalendarUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalendarUsers_Calendars_CalendarId",
                table: "CalendarUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarUsers_Calendars_CalendarId",
                table: "CalendarUsers",
                column: "CalendarId",
                principalTable: "Calendars",
                principalColumn: "CalendarId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalendarUsers_Calendars_CalendarId",
                table: "CalendarUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarUsers_Calendars_CalendarId",
                table: "CalendarUsers",
                column: "CalendarId",
                principalTable: "Calendars",
                principalColumn: "CalendarId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
