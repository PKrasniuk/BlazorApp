using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace BlazorApp.DAL.Migrations.ApplicationDb
{
    public partial class AddDbLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "logs",
                columns: table => new
                {
                    message = table.Column<string>(type: "text", nullable: false),
                    message_template = table.Column<string>(type: "text", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(nullable: false),
                    exception = table.Column<string>(type: "text", nullable: true),
                    log_event = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "logs");
        }
    }
}
