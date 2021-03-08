using Microsoft.EntityFrameworkCore.Migrations;

namespace Intellegens.Commons.Search.Tests.SearchTests.Setup.Migrations.Postgres
{
    public partial class ChildEntityTextField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "SearchTestChildEntities",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Text",
                table: "SearchTestChildEntities");
        }
    }
}
