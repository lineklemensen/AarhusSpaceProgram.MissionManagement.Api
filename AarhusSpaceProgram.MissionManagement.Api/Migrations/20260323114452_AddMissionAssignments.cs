using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AarhusSpaceProgram.MissionManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMissionAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CelestialBodyId",
                table: "Missions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LaunchpadId",
                table: "Missions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "Missions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RocketId",
                table: "Missions",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Managers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CelestialBodyId", "LaunchpadId", "ManagerId", "RocketId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CelestialBodyId", "LaunchpadId", "ManagerId", "RocketId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Missions_CelestialBodyId",
                table: "Missions",
                column: "CelestialBodyId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_LaunchpadId",
                table: "Missions",
                column: "LaunchpadId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_ManagerId",
                table: "Missions",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_RocketId",
                table: "Missions",
                column: "RocketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_CelestialBodies_CelestialBodyId",
                table: "Missions",
                column: "CelestialBodyId",
                principalTable: "CelestialBodies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Launchpads_LaunchpadId",
                table: "Missions",
                column: "LaunchpadId",
                principalTable: "Launchpads",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Managers_ManagerId",
                table: "Missions",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Rockets_RocketId",
                table: "Missions",
                column: "RocketId",
                principalTable: "Rockets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Missions_CelestialBodies_CelestialBodyId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Launchpads_LaunchpadId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Managers_ManagerId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Rockets_RocketId",
                table: "Missions");

            migrationBuilder.DropIndex(
                name: "IX_Missions_CelestialBodyId",
                table: "Missions");

            migrationBuilder.DropIndex(
                name: "IX_Missions_LaunchpadId",
                table: "Missions");

            migrationBuilder.DropIndex(
                name: "IX_Missions_ManagerId",
                table: "Missions");

            migrationBuilder.DropIndex(
                name: "IX_Missions_RocketId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "CelestialBodyId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "LaunchpadId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "RocketId",
                table: "Missions");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
