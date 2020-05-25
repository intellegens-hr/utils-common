using Microsoft.EntityFrameworkCore.Migrations;

namespace Intellegens.Commons.Tests.SearchTests.Setup.Migrations.Sqlite
{
    public partial class SiblingEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SiblingId",
                table: "SearchTestEntities",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchTestEntities_SiblingId",
                table: "SearchTestEntities",
                column: "SiblingId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SearchTestEntities_SiblingId",
                table: "SearchTestEntities");

            migrationBuilder.DropColumn(
                name: "SiblingId",
                table: "SearchTestEntities");
        }
    }
}
