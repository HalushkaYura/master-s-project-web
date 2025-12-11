using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartClass.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAllRelationsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeedbackHtml",
                table: "Grades");

            migrationBuilder.AlterColumn<decimal>(
                name: "Score",
                table: "Grades",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Grades",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Channels",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_ClassroomId_AssignmentId",
                table: "Files",
                columns: new[] { "ClassroomId", "AssignmentId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Submissions_SubmissionId",
                table: "Grades",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Assignments_AssignmentId",
                table: "Submissions",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Submissions_SubmissionId",
                table: "Grades");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Assignments_AssignmentId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Files_ClassroomId_AssignmentId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Channels");

            migrationBuilder.AlterColumn<decimal>(
                name: "Score",
                table: "Grades",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackHtml",
                table: "Grades",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
