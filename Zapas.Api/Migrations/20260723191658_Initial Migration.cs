using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zapas.Api.Migrations;

/// <inheritdoc />
public partial class InitialMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Sessions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OwnerUserId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                StartTime = table.Column<long>(type: "INTEGER", nullable: false),
                TotalDistanceMeters = table.Column<double>(type: "REAL", nullable: false),
                TotalDuration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                AveragePaceSecondsPerKm = table.Column<double>(type: "REAL", nullable: false),
                CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                AverageHeartRate = table.Column<byte>(type: "INTEGER", nullable: true),
                MaxHeartRate = table.Column<byte>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Sessions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "RunIntervals",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                LapNumber = table.Column<int>(type: "INTEGER", nullable: false),
                DistanceMeters = table.Column<double>(type: "REAL", nullable: false),
                Duration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                AveragePaceSecondsPerKm = table.Column<double>(type: "REAL", nullable: false),
                AverageHeartRate = table.Column<byte>(type: "INTEGER", nullable: true),
                MaxHeartRate = table.Column<byte>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RunIntervals", x => x.Id);
                table.ForeignKey(
                    name: "FK_RunIntervals_Sessions_SessionId",
                    column: x => x.SessionId,
                    principalTable: "Sessions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_RunIntervals_SessionId",
            table: "RunIntervals",
            column: "SessionId");

        migrationBuilder.CreateIndex(
            name: "IX_Sessions_OwnerUserId",
            table: "Sessions",
            column: "OwnerUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Sessions_StartTime",
            table: "Sessions",
            column: "StartTime");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RunIntervals");

        migrationBuilder.DropTable(
            name: "Sessions");
    }
}
