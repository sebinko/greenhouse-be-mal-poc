using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ControlStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Watering = table.Column<bool>(type: "bit", nullable: false),
                    Light = table.Column<bool>(type: "bit", nullable: false),
                    Fertilizer = table.Column<bool>(type: "bit", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SensorReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AirTemperature = table.Column<double>(type: "float", nullable: false),
                    AirHumidity = table.Column<double>(type: "float", nullable: false),
                    SoilHumidity = table.Column<double>(type: "float", nullable: false),
                    LightLevel = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReadings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ControlStates");

            migrationBuilder.DropTable(
                name: "SensorReadings");
        }
    }
}
