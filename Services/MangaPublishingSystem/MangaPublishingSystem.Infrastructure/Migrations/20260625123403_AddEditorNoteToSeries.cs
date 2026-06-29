using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaPublishingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEditorNoteToSeries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EditorNote",
                table: "Series",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditorNote",
                table: "Series");
        }
    }
}
