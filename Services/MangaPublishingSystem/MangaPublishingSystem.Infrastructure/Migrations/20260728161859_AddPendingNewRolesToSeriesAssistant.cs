using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaPublishingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingNewRolesToSeriesAssistant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CitizenId",
                table: "User",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CitizenIdIssueDate",
                table: "User",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenIdIssuePlace",
                table: "User",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingNewRoles",
                table: "Series_Assistant",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContractRejectionCount",
                table: "Series",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ContractFileUrl",
                table: "Contract",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "Contract",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "Contract",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishDate",
                table: "Chapter",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicationSchedule",
                table: "BoardVote",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContractTemplate",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTemplate", x => x.TemplateId);
                    table.ForeignKey(
                        name: "FK_ContractTemplate_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contract_TemplateId",
                table: "Contract",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTemplate_CreatedByUserId",
                table: "ContractTemplate",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contract_ContractTemplate_TemplateId",
                table: "Contract",
                column: "TemplateId",
                principalTable: "ContractTemplate",
                principalColumn: "TemplateId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contract_ContractTemplate_TemplateId",
                table: "Contract");

            migrationBuilder.DropTable(
                name: "ContractTemplate");

            migrationBuilder.DropIndex(
                name: "IX_Contract_TemplateId",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "CitizenId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CitizenIdIssueDate",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CitizenIdIssuePlace",
                table: "User");

            migrationBuilder.DropColumn(
                name: "PendingNewRoles",
                table: "Series_Assistant");

            migrationBuilder.DropColumn(
                name: "ContractRejectionCount",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "ContractFileUrl",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "PublishDate",
                table: "Chapter");

            migrationBuilder.DropColumn(
                name: "PublicationSchedule",
                table: "BoardVote");
        }
    }
}
