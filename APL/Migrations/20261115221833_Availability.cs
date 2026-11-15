using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AcademicManagement.Migrations
{
    /// <inheritdoc />
    public partial class Availability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AvailabilityBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYear = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    SlotNumber = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilityBlocks", x => x.Id);
                    table.CheckConstraint("CK_AvailabilityBlocks_Day", "\"DayOfWeek\" BETWEEN 1 AND 6");
                    table.ForeignKey(
                        name: "FK_AvailabilityBlocks_ProfessorProfiles_ProfessorProfileId",
                        column: x => x.ProfessorProfileId,
                        principalTable: "ProfessorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AvailabilityPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYear = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    MaxTeachingDays = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilityPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvailabilityPreferences_ProfessorProfiles_ProfessorProfileId",
                        column: x => x.ProfessorProfileId,
                        principalTable: "ProfessorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExternalCommitments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYear = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalCommitments", x => x.Id);
                    table.CheckConstraint("CK_ExternalCommitments_Day", "\"DayOfWeek\" BETWEEN 1 AND 6");
                    table.ForeignKey(
                        name: "FK_ExternalCommitments_ProfessorProfiles_ProfessorProfileId",
                        column: x => x.ProfessorProfileId,
                        principalTable: "ProfessorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeSlots",
                columns: table => new
                {
                    SlotNumber = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    IsLunchBreak = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlots", x => x.SlotNumber);
                });

            migrationBuilder.InsertData(
                table: "TimeSlots",
                columns: new[] { "SlotNumber", "EndTime", "IsLunchBreak", "StartTime" },
                values: new object[,]
                {
                    { 1, new TimeSpan(0, 9, 30, 0, 0), false, new TimeSpan(0, 8, 0, 0, 0) },
                    { 2, new TimeSpan(0, 11, 15, 0, 0), false, new TimeSpan(0, 9, 45, 0, 0) },
                    { 3, new TimeSpan(0, 13, 0, 0, 0), false, new TimeSpan(0, 11, 30, 0, 0) },
                    { 4, new TimeSpan(0, 14, 30, 0, 0), true, new TimeSpan(0, 13, 0, 0, 0) },
                    { 5, new TimeSpan(0, 16, 0, 0, 0), false, new TimeSpan(0, 14, 30, 0, 0) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityBlocks_ProfessorProfileId_AcademicYear_DayOfWee~",
                table: "AvailabilityBlocks",
                columns: new[] { "ProfessorProfileId", "AcademicYear", "DayOfWeek", "SlotNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityPreferences_ProfessorProfileId_AcademicYear",
                table: "AvailabilityPreferences",
                columns: new[] { "ProfessorProfileId", "AcademicYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCommitments_ProfessorProfileId",
                table: "ExternalCommitments",
                column: "ProfessorProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailabilityBlocks");

            migrationBuilder.DropTable(
                name: "AvailabilityPreferences");

            migrationBuilder.DropTable(
                name: "ExternalCommitments");

            migrationBuilder.DropTable(
                name: "TimeSlots");
        }
    }
}
