using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AarhusSpaceProgram.MissionManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaAndSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaxSupportedWeight",
                table: "Launchpads",
                newName: "MaxSupportedWeightKg");

            migrationBuilder.RenameColumn(
                name: "DistanceValueAU",
                table: "CelestialBodies",
                newName: "DistanceValueToParentAU");

            migrationBuilder.AddColumn<string>(
                name: "PadCode",
                table: "Launchpads",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Astronauts",
                columns: new[] { "Id", "HoursInSimulation", "HoursInSpace", "Name", "Paygrade", "Rank" },
                values: new object[,]
                {
                    { 1, 500, 100, "Neil Legstrong", "2-A", "Astronaut" },
                    { 2, 600, 150, "Buzz Lightyear", "3-A", "Pilot" },
                    { 3, 700, 200, "Sally Ride", "3-A", "MissionSpecialist" }
                });

            migrationBuilder.InsertData(
                table: "CelestialBodies",
                columns: new[] { "Id", "BodyType", "DistanceValueToParentAU", "Name", "ParentId", "PlanetClass" },
                values: new object[,]
                {
                    { 1, "Planet", 1.0, "Earth", null, "Rocky" },
                    { 3, "Planet", 1.524, "Mars", null, "Rocky" }
                });

            migrationBuilder.InsertData(
                table: "Launchpads",
                columns: new[] { "Id", "Location", "MaxSupportedWeightKg", "PadCode", "Status" },
                values: new object[,]
                {
                    { 1, "Kennedy Space Center, Florida, USA", 63800, "LC-39A", "Operational" },
                    { 2, "Cape Canaveral Space Force Station, Florida, USA", 17440, "SLC-41", "UnderMaintenance" }
                });

            migrationBuilder.InsertData(
                table: "Managers",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Dean Kranz" },
                    { 2, "Ellen Ripley" }
                });

            migrationBuilder.InsertData(
                table: "Missions",
                columns: new[] { "Id", "DurationHours", "LaunchDate", "Name", "Status", "Type" },
                values: new object[,]
                {
                    { 1, 4872, new DateOnly(2020, 7, 30), "Mars 2020", "Completed", "Landing" },
                    { 2, 613, new DateOnly(2022, 11, 16), "Artemis I", "Completed", "Orbit" }
                });

            migrationBuilder.InsertData(
                table: "Rockets",
                columns: new[] { "Id", "CrewCapacity", "FuelCapacityKg", "Name", "NumberOfStages", "PayloadCapacityKg", "WeightKg" },
                values: new object[,]
                {
                    { 1, 0, 284000, "Atlas V 541", 2, 17440, 49000 },
                    { 2, 4, 2500000, "Space Launch System Block 1", 2, 95000, 130000 }
                });

            migrationBuilder.InsertData(
                table: "Scientists",
                columns: new[] { "Id", "HireDate", "Name", "Specialty", "Title" },
                values: new object[,]
                {
                    { 1, new DateTime(2010, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Howard Wolowitz", "Rocket Propulsion", "Aerospace Engineer" },
                    { 2, new DateTime(2012, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Werner von Schwartz", "Planetary Science", "Astrophysicist" }
                });

            migrationBuilder.InsertData(
                table: "CelestialBodies",
                columns: new[] { "Id", "BodyType", "DistanceValueToParentAU", "Name", "ParentId", "PlanetClass" },
                values: new object[] { 2, "Moon", 0.0025699999999999998, "Moon", 1, null });

            migrationBuilder.InsertData(
                table: "MissionAstronautAssignments",
                columns: new[] { "AstronautId", "MissionId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 2 }
                });

            migrationBuilder.InsertData(
                table: "MissionScientistAssignments",
                columns: new[] { "MissionId", "ScientistId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Launchpads_PadCode",
                table: "Launchpads",
                column: "PadCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Launchpads_PadCode",
                table: "Launchpads");

            migrationBuilder.DeleteData(
                table: "CelestialBodies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CelestialBodies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Launchpads",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Launchpads",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Managers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Managers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MissionAstronautAssignments",
                keyColumns: new[] { "AstronautId", "MissionId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "MissionAstronautAssignments",
                keyColumns: new[] { "AstronautId", "MissionId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "MissionAstronautAssignments",
                keyColumns: new[] { "AstronautId", "MissionId" },
                keyValues: new object[] { 3, 2 });

            migrationBuilder.DeleteData(
                table: "MissionScientistAssignments",
                keyColumns: new[] { "MissionId", "ScientistId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "MissionScientistAssignments",
                keyColumns: new[] { "MissionId", "ScientistId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "Rockets",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Rockets",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Astronauts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Astronauts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Astronauts",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CelestialBodies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Scientists",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Scientists",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "PadCode",
                table: "Launchpads");

            migrationBuilder.RenameColumn(
                name: "MaxSupportedWeightKg",
                table: "Launchpads",
                newName: "MaxSupportedWeight");

            migrationBuilder.RenameColumn(
                name: "DistanceValueToParentAU",
                table: "CelestialBodies",
                newName: "DistanceValueAU");
        }
    }
}
