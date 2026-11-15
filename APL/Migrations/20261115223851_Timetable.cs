using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicManagement.Migrations
{
    /// <inheritdoc />
    public partial class Timetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Building = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HasComputers = table.Column<bool>(type: "boolean", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.CheckConstraint("CK_Rooms_Capacity", "\"Capacity\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "TimetableRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYear = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    Semester = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SolverStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SessionsTotal = table.Column<int>(type: "integer", nullable: false),
                    SessionsPlaced = table.Column<int>(type: "integer", nullable: false),
                    Penalty = table.Column<int>(type: "integer", nullable: false),
                    TimeLimitSeconds = table.Column<int>(type: "integer", nullable: false),
                    SolveSeconds = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimetableEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimetableRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    SlotNumber = table.Column<int>(type: "integer", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    Parity = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimetableEntries_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimetableEntries_TeachingAssignments_TeachingAssignmentId",
                        column: x => x.TeachingAssignmentId,
                        principalTable: "TeachingAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimetableEntries_TimetableRuns_TimetableRunId",
                        column: x => x.TimetableRunId,
                        principalTable: "TimetableRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimetableUnplaced",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimetableRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sessions = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableUnplaced", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimetableUnplaced_TeachingAssignments_TeachingAssignmentId",
                        column: x => x.TeachingAssignmentId,
                        principalTable: "TeachingAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimetableUnplaced_TimetableRuns_TimetableRunId",
                        column: x => x.TimetableRunId,
                        principalTable: "TimetableRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Building_Code",
                table: "Rooms",
                columns: new[] { "Building", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntries_RoomId",
                table: "TimetableEntries",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntries_TeachingAssignmentId",
                table: "TimetableEntries",
                column: "TeachingAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntries_TimetableRunId_DayOfWeek_SlotNumber",
                table: "TimetableEntries",
                columns: new[] { "TimetableRunId", "DayOfWeek", "SlotNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_TimetableRuns_AcademicYear_Semester",
                table: "TimetableRuns",
                columns: new[] { "AcademicYear", "Semester" });

            migrationBuilder.CreateIndex(
                name: "IX_TimetableUnplaced_TeachingAssignmentId",
                table: "TimetableUnplaced",
                column: "TeachingAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableUnplaced_TimetableRunId",
                table: "TimetableUnplaced",
                column: "TimetableRunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimetableEntries");

            migrationBuilder.DropTable(
                name: "TimetableUnplaced");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "TimetableRuns");
        }
    }
}
