using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CloudHeavenApi.Migrations
{
    public partial class Cloud : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CloudHeaven_Badge",
                columns: table => new
                {
                    BadgeId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BadgeName = table.Column<string>(nullable: true),
                    BadgeLink = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudHeaven_Badge", x => x.BadgeId);
                });

            migrationBuilder.CreateTable(
                name: "CloudHeaven_WebAccount",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    NickName = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    JoinTime = table.Column<long>(nullable: false),
                    Admin = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudHeaven_WebAccount", x => x.Uuid);
                });

            migrationBuilder.CreateTable(
                name: "CloudHeaven_PersonBadges",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(nullable: false),
                    BadgeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudHeaven_PersonBadges", x => new { x.Uuid, x.BadgeId });
                    table.ForeignKey(
                        name: "FK_CloudHeaven_PersonBadges_CloudHeaven_Badge_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "CloudHeaven_Badge",
                        principalColumn: "BadgeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CloudHeaven_PersonBadges_CloudHeaven_WebAccount_Uuid",
                        column: x => x.Uuid,
                        principalTable: "CloudHeaven_WebAccount",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CloudHeaven_PersonBadges_BadgeId",
                table: "CloudHeaven_PersonBadges",
                column: "BadgeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CloudHeaven_PersonBadges");

            migrationBuilder.DropTable(
                name: "CloudHeaven_Badge");

            migrationBuilder.DropTable(
                name: "CloudHeaven_WebAccount");
        }
    }
}
