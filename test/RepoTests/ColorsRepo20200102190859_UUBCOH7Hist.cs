using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Colors.Migrations
{
    public partial class UUBCOH7Hist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo_history");

            migrationBuilder.CreateTable(
                name: "Color",
                schema: "dbo_history",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    SysStart = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(unicode: false, maxLength: 30, nullable: true),
                    SysEnd = table.Column<DateTime>(nullable: false),
                    SysUser = table.Column<string>(nullable: true),
                    SysUserNext = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Color", x => new { x.Id, x.SysStart });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Color",
                schema: "dbo_history");
        }
    }
}