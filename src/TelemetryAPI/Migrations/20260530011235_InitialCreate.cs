using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelemetryAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnergyReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnergyLevel = table.Column<double>(type: "float", nullable: false),
                    VoltageV = table.Column<double>(type: "float", nullable: false),
                    GeneratorOnline = table.Column<bool>(type: "bit", nullable: false),
                    PowerConsumptionKw = table.Column<double>(type: "float", nullable: false),
                    SensorId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BaseLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Processed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnergyReadings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelemetryReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Temperature = table.Column<double>(type: "float", nullable: false),
                    HeartRate = table.Column<int>(type: "int", nullable: false),
                    OperatorId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OxygenLevel = table.Column<double>(type: "float", nullable: false),
                    SensorId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BaseLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Processed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryReadings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnergyReadings_BaseLocation",
                table: "EnergyReadings",
                column: "BaseLocation");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyReadings_Timestamp",
                table: "EnergyReadings",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryReadings_OperatorId",
                table: "TelemetryReadings",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryReadings_Timestamp",
                table: "TelemetryReadings",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnergyReadings");

            migrationBuilder.DropTable(
                name: "TelemetryReadings");
        }
    }
}
