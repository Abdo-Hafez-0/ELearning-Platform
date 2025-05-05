using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearningPlatform.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Lessons_LessonId",
                table: "Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_InstructorID",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAssignmentResults_AssignmentOptions_SelectedOptionID",
                table: "UserAssignmentResults");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAssignmentResults_Assignments_TestID",
                table: "UserAssignmentResults");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Lessons_LessonId",
                table: "Assignments",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_InstructorID",
                table: "Courses",
                column: "InstructorID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAssignmentResults_AssignmentOptions_SelectedOptionID",
                table: "UserAssignmentResults",
                column: "SelectedOptionID",
                principalTable: "AssignmentOptions",
                principalColumn: "OptionID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAssignmentResults_Assignments_TestID",
                table: "UserAssignmentResults",
                column: "TestID",
                principalTable: "Assignments",
                principalColumn: "TestID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Lessons_LessonId",
                table: "Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_InstructorID",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAssignmentResults_AssignmentOptions_SelectedOptionID",
                table: "UserAssignmentResults");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAssignmentResults_Assignments_TestID",
                table: "UserAssignmentResults");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Lessons_LessonId",
                table: "Assignments",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_InstructorID",
                table: "Courses",
                column: "InstructorID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAssignmentResults_AssignmentOptions_SelectedOptionID",
                table: "UserAssignmentResults",
                column: "SelectedOptionID",
                principalTable: "AssignmentOptions",
                principalColumn: "OptionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAssignmentResults_Assignments_TestID",
                table: "UserAssignmentResults",
                column: "TestID",
                principalTable: "Assignments",
                principalColumn: "TestID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
