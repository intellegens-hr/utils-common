using Microsoft.EntityFrameworkCore.Migrations;

namespace Intellegens.Commons.Search.Tests.SearchTests.Setup.Migrations.Sqlite
{
    public partial class ChildEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchTestChildEntities",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentId = table.Column<int>(nullable: false),
                    TestingSessionId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchTestChildEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchTestChildEntities_SearchTestEntities_ParentId",
                        column: x => x.ParentId,
                        principalTable: "SearchTestEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchTestChildEntities_ParentId",
                table: "SearchTestChildEntities",
                column: "ParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchTestChildEntities");
        }
    }
}
