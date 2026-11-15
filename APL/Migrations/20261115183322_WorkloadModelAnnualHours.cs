using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicManagement.Migrations
{
    /// <inheritdoc />
    public partial class WorkloadModelAnnualHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PositionNorms_Position_AcademicYear",
                table: "PositionNorms");

            migrationBuilder.DropIndex(
                name: "IX_ConversionCoefficients_AcademicYear_ActivityType",
                table: "ConversionCoefficients");

            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "PositionNorms");

            migrationBuilder.DropColumn(
                name: "WeeksPerSemester",
                table: "PositionNorms");

            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "ConversionCoefficients");

            migrationBuilder.DropColumn(
                name: "WeeklyHours",
                table: "Contracts");

            migrationBuilder.AlterColumn<decimal>(
                name: "MinHours",
                table: "PositionNorms",
                type: "numeric(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxHours",
                table: "PositionNorms",
                type: "numeric(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "BaseNormHours",
                table: "PositionNorms",
                type: "numeric(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkloadModelId",
                table: "PositionNorms",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "WorkloadModelId",
                table: "ConversionCoefficients",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualHours",
                table: "Contracts",
                type: "numeric(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "WorkloadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYear = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkloadModels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PositionNorms_WorkloadModelId_Position",
                table: "PositionNorms",
                columns: new[] { "WorkloadModelId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversionCoefficients_WorkloadModelId_ActivityType",
                table: "ConversionCoefficients",
                columns: new[] { "WorkloadModelId", "ActivityType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkloadModels_AcademicYear",
                table: "WorkloadModels",
                column: "AcademicYear",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConversionCoefficients_WorkloadModels_WorkloadModelId",
                table: "ConversionCoefficients",
                column: "WorkloadModelId",
                principalTable: "WorkloadModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionNorms_WorkloadModels_WorkloadModelId",
                table: "PositionNorms",
                column: "WorkloadModelId",
                principalTable: "WorkloadModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversionCoefficients_WorkloadModels_WorkloadModelId",
                table: "ConversionCoefficients");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionNorms_WorkloadModels_WorkloadModelId",
                table: "PositionNorms");

            migrationBuilder.DropTable(
                name: "WorkloadModels");

            migrationBuilder.DropIndex(
                name: "IX_PositionNorms_WorkloadModelId_Position",
                table: "PositionNorms");

            migrationBuilder.DropIndex(
                name: "IX_ConversionCoefficients_WorkloadModelId_ActivityType",
                table: "ConversionCoefficients");

            migrationBuilder.DropColumn(
                name: "WorkloadModelId",
                table: "PositionNorms");

            migrationBuilder.DropColumn(
                name: "WorkloadModelId",
                table: "ConversionCoefficients");

            migrationBuilder.DropColumn(
                name: "AnnualHours",
                table: "Contracts");

            migrationBuilder.AlterColumn<decimal>(
                name: "MinHours",
                table: "PositionNorms",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(7,2)",
                oldPrecision: 7,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxHours",
                table: "PositionNorms",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(7,2)",
                oldPrecision: 7,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "BaseNormHours",
                table: "PositionNorms",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(7,2)",
                oldPrecision: 7,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "AcademicYear",
                table: "PositionNorms",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WeeksPerSemester",
                table: "PositionNorms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AcademicYear",
                table: "ConversionCoefficients",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "WeeklyHours",
                table: "Contracts",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_PositionNorms_Position_AcademicYear",
                table: "PositionNorms",
                columns: new[] { "Position", "AcademicYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversionCoefficients_AcademicYear_ActivityType",
                table: "ConversionCoefficients",
                columns: new[] { "AcademicYear", "ActivityType" },
                unique: true);
        }
    }
}
