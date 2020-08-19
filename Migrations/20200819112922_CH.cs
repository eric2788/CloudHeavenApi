using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CloudHeavenApi.Migrations
{
    public partial class CH : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "CloudHeaven_Badge",
                table => new
                {
                    BadgeId = table.Column<int>()
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BadgeName = table.Column<string>(nullable: true),
                    BadgeLink = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_CloudHeaven_Badge", x => x.BadgeId); });

            migrationBuilder.CreateTable(
                "CloudHeaven_WebAccount",
                table => new
                {
                    Uuid = table.Column<Guid>(),
                    UserName = table.Column<string>(nullable: true),
                    NickName = table.Column<string>(nullable: true),
                    Admin = table.Column<bool>()
                },
                constraints: table => { table.PrimaryKey("PK_CloudHeaven_WebAccount", x => x.Uuid); });

            migrationBuilder.CreateTable(
                "CloudHeaven_PersonBadges",
                table => new
                {
                    Uuid = table.Column<Guid>(),
                    BadgeId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudHeaven_PersonBadges", x => new {x.Uuid, x.BadgeId});
                    table.ForeignKey(
                        "FK_CloudHeaven_PersonBadges_CloudHeaven_Badge_BadgeId",
                        x => x.BadgeId,
                        "CloudHeaven_Badge",
                        "BadgeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_CloudHeaven_PersonBadges_CloudHeaven_WebAccount_Uuid",
                        x => x.Uuid,
                        "CloudHeaven_WebAccount",
                        "Uuid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_CloudHeaven_PersonBadges_BadgeId",
                "CloudHeaven_PersonBadges",
                "BadgeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "CloudHeaven_PersonBadges");

            migrationBuilder.DropTable(
                "CloudHeaven_Badge");

            migrationBuilder.DropTable(
                "CloudHeaven_WebAccount");
        }
    }
}