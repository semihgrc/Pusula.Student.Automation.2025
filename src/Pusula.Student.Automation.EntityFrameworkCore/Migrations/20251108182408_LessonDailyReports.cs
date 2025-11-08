using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pusula.Student.Automation.Migrations
{
    /// <inheritdoc />
    public partial class LessonDailyReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "AppLessonEnrollments"
                ADD COLUMN IF NOT EXISTS "FinalGrade" numeric(5,2);
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "AppLessonEnrollments"
                ADD COLUMN IF NOT EXISTS "MidtermGrade" numeric(5,2);
                """);

            migrationBuilder.CreateTable(
                name: "AppLessonDailyReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLessonDailyReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppLessonDailyReportEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonDailyReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPresent = table.Column<bool>(type: "boolean", nullable: false),
                    DailyGrade = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    DailyComment = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLessonDailyReportEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppLessonDailyReportEntries_AppLessonDailyReports_LessonDai~",
                        column: x => x.LessonDailyReportId,
                        principalTable: "AppLessonDailyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppLessonDailyReportEntries_LessonDailyReportId_StudentId",
                table: "AppLessonDailyReportEntries",
                columns: new[] { "LessonDailyReportId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppLessonDailyReports_LessonId_Date",
                table: "AppLessonDailyReports",
                columns: new[] { "LessonId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppLessonDailyReportEntries");

            migrationBuilder.DropTable(
                name: "AppLessonDailyReports");

            migrationBuilder.DropColumn(
                name: "FinalGrade",
                table: "AppLessonEnrollments");

            migrationBuilder.DropColumn(
                name: "MidtermGrade",
                table: "AppLessonEnrollments");
        }
    }
}
