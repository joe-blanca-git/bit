using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace apiBit.Migrations
{
    /// <inheritdoc />
    public partial class TabelasPlanosPermissoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlanId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ApplicationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanApplications_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanApplications_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PlanMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlanId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ApplicationMenuId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanMenus_ApplicationMenus_ApplicationMenuId",
                        column: x => x.ApplicationMenuId,
                        principalTable: "ApplicationMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanMenus_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PlanSubMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlanId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ApplicationSubMenuId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanSubMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanSubMenus_ApplicationSubMenus_ApplicationSubMenuId",
                        column: x => x.ApplicationSubMenuId,
                        principalTable: "ApplicationSubMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanSubMenus_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PlanApplications_ApplicationId",
                table: "PlanApplications",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanApplications_PlanId_ApplicationId",
                table: "PlanApplications",
                columns: new[] { "PlanId", "ApplicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanMenus_ApplicationMenuId",
                table: "PlanMenus",
                column: "ApplicationMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanMenus_PlanId_ApplicationMenuId",
                table: "PlanMenus",
                columns: new[] { "PlanId", "ApplicationMenuId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanSubMenus_ApplicationSubMenuId",
                table: "PlanSubMenus",
                column: "ApplicationSubMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanSubMenus_PlanId_ApplicationSubMenuId",
                table: "PlanSubMenus",
                columns: new[] { "PlanId", "ApplicationSubMenuId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanApplications");

            migrationBuilder.DropTable(
                name: "PlanMenus");

            migrationBuilder.DropTable(
                name: "PlanSubMenus");
        }
    }
}
