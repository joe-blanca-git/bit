using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace apiBit.Migrations
{
    /// <inheritdoc />
    public partial class CriandoTabelaEnderecos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonAddress_People_PersonId",
                table: "PersonAddress");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonAddress",
                table: "PersonAddress");

            migrationBuilder.RenameTable(
                name: "PersonAddress",
                newName: "PersonAddresses");

            migrationBuilder.RenameIndex(
                name: "IX_PersonAddress_PersonId",
                table: "PersonAddresses",
                newName: "IX_PersonAddresses_PersonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonAddresses",
                table: "PersonAddresses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonAddresses_People_PersonId",
                table: "PersonAddresses",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonAddresses_People_PersonId",
                table: "PersonAddresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonAddresses",
                table: "PersonAddresses");

            migrationBuilder.RenameTable(
                name: "PersonAddresses",
                newName: "PersonAddress");

            migrationBuilder.RenameIndex(
                name: "IX_PersonAddresses_PersonId",
                table: "PersonAddress",
                newName: "IX_PersonAddress_PersonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonAddress",
                table: "PersonAddress",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonAddress_People_PersonId",
                table: "PersonAddress",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
