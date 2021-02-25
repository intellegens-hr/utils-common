using Microsoft.EntityFrameworkCore.Migrations;

namespace Intellegens.Commons.Search.Tests.SearchTests.Setup.Migrations.Postgres
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

            migrationBuilder.AddForeignKey(
                name: "FK_SearchTestEntities_SearchTestEntities_SiblingId",
                table: "SearchTestEntities",
                column: "SiblingId",
                principalTable: "SearchTestEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SearchTestEntities_SearchTestEntities_SiblingId",
                table: "SearchTestEntities");

            migrationBuilder.DropIndex(
                name: "IX_SearchTestEntities_SiblingId",
                table: "SearchTestEntities");

            migrationBuilder.DropColumn(
                name: "SiblingId",
                table: "SearchTestEntities");
        }
    }
}
