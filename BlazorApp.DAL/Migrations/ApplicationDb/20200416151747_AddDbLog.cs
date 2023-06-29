using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BlazorApp.DAL.Migrations.ApplicationDb;

public partial class AddDbLog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "logs",
            table => new
            {
                message = table.Column<string>("text", nullable: false),
                message_template = table.Column<string>("text", nullable: false),
                level = table.Column<int>("integer", nullable: false),
                timestamp = table.Column<DateTimeOffset>(nullable: false),
                exception = table.Column<string>("text", nullable: true),
                log_event = table.Column<string>("jsonb", nullable: true)
            },
            constraints: table => { });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "logs");
    }
}