using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaPublishingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPhoneAndAvatar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wallet_UserId",
                table: "Wallet");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Wallet",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "Wallet",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "User");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EditorRecommendedBudget",
                table: "Series",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<string>(
                name: "MangakaSubmissionNote",
                table: "Series",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BoardVotingConfig",
                columns: table => new
                {
                    ConfigId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AutoResolveHours = table.Column<int>(type: "int", nullable: false, defaultValue: 48),
                    ApprovalThresholdPercent = table.Column<int>(type: "int", nullable: false, defaultValue: 51),
                    ClearVotesOnResubmit = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    BoardRoleId = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    ChairUserId = table.Column<int>(type: "int", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardVotingConfig", x => x.ConfigId);
                });

            migrationBuilder.CreateTable(
                name: "Series_Assistant",
                columns: table => new
                {
                    SeriesId = table.Column<int>(type: "int", nullable: false),
                    AssistantId = table.Column<int>(type: "int", nullable: false),
                    RoleInTeam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series_Assistant", x => new { x.SeriesId, x.AssistantId });
                    table.ForeignKey(
                        name: "FK_Series_Assistant_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "SeriesId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Series_Assistant_User_AssistantId",
                        column: x => x.AssistantId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_Kind",
                table: "Wallet",
                column: "Kind",
                unique: true,
                filter: "[Kind] = 'PlatformTreasury'");

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_UserId",
                table: "Wallet",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Series_Assistant_AssistantId",
                table: "Series_Assistant",
                column: "AssistantId");

            migrationBuilder.CreateIndex(
                name: "IX_Series_Assistant_SeriesId_Status",
                table: "Series_Assistant",
                columns: new[] { "SeriesId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardVotingConfig");

            migrationBuilder.DropTable(
                name: "Series_Assistant");

            migrationBuilder.DropIndex(
                name: "IX_Wallet_Kind",
                table: "Wallet");

            migrationBuilder.DropIndex(
                name: "IX_Wallet_UserId",
                table: "Wallet");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "Wallet");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "User");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "User");

            migrationBuilder.DropColumn(
                name: "EditorRecommendedBudget",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "MangakaSubmissionNote",
                table: "Series");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Wallet",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_UserId",
                table: "Wallet",
                column: "UserId",
                unique: true);
        }
    }
}
