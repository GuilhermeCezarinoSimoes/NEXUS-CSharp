using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlertsAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SensorType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SensorId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Acknowledged = table.Column<bool>(type: "bit", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlertId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BaseLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyIncidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AffectedSystems = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlertCount = table.Column<int>(type: "int", nullable: false),
                    IsolationProtocolTriggered = table.Column<bool>(type: "bit", nullable: false),
                    IsolationTriggeredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsibleOperator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlertId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BaseLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyIncidents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AlertId",
                table: "Alerts",
                column: "AlertId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_ReceivedAt",
                table: "Alerts",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Status",
                table: "Alerts",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "EmergencyIncidents");
        }
    }
}
