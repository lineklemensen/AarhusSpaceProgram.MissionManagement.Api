using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AarhusSpaceProgram.MissionManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class Experiments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Experiments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experiments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExperimentAstronautAssignments",
                columns: table => new
                {
                    ExperimentId = table.Column<int>(type: "int", nullable: false),
                    AstronautId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperimentAstronautAssignments", x => new { x.ExperimentId, x.AstronautId });
                    table.ForeignKey(
                        name: "FK_ExperimentAstronautAssignments_Astronauts_AstronautId",
                        column: x => x.AstronautId,
                        principalTable: "Astronauts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExperimentAstronautAssignments_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExperimentScientistAssignments",
                columns: table => new
                {
                    ExperimentId = table.Column<int>(type: "int", nullable: false),
                    ScientistId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperimentScientistAssignments", x => new { x.ExperimentId, x.ScientistId });
                    table.ForeignKey(
                        name: "FK_ExperimentScientistAssignments_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExperimentScientistAssignments_Scientists_ScientistId",
                        column: x => x.ScientistId,
                        principalTable: "Scientists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExperimentsOnMissions",
                columns: table => new
                {
                    MissionId = table.Column<int>(type: "int", nullable: false),
                    ExperimentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperimentsOnMissions", x => new { x.MissionId, x.ExperimentId });
                    table.ForeignKey(
                        name: "FK_ExperimentsOnMissions_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExperimentsOnMissions_Missions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentAstronautAssignments_AstronautId",
                table: "ExperimentAstronautAssignments",
                column: "AstronautId");

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentScientistAssignments_ScientistId",
                table: "ExperimentScientistAssignments",
                column: "ScientistId");

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentsOnMissions_ExperimentId",
                table: "ExperimentsOnMissions",
                column: "ExperimentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExperimentAstronautAssignments");

            migrationBuilder.DropTable(
                name: "ExperimentScientistAssignments");

            migrationBuilder.DropTable(
                name: "ExperimentsOnMissions");

            migrationBuilder.DropTable(
                name: "Experiments");
        }
    }
}
