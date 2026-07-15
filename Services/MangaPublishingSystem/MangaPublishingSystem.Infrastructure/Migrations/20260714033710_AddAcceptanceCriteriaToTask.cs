using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaPublishingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAcceptanceCriteriaToTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcceptanceCriteria",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptanceCriteria",
                table: "Tasks");
        }
    }
}
