using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelMate.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiAuditEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiAuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Operation = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EstimatedTokens = table.Column<int>(type: "int", nullable: false),
                    EstimatedCostUsd = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    LatencyMilliseconds = table.Column<int>(type: "int", nullable: false),
                    Succeeded = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAuditEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiAuditEvents_OccurredAt",
                table: "AiAuditEvents",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiAuditEvents_TaskName_OccurredAt",
                table: "AiAuditEvents",
                columns: new[] { "TaskName", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiAuditEvents");
        }
    }
}
