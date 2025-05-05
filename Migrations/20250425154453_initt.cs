using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearningPlatform.Migrations
{
    /// <inheritdoc />
    public partial class initt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAssignmentResults_AssignmentOptions_SelectedOptionID",
                table: "UserAssignmentResults");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAssignmentResults_AssignmentOptions_SelectedOptionID",
                table: "UserAssignmentResults",
                column: "SelectedOptionID",
                principalTable: "AssignmentOptions",
                principalColumn: "OptionID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAssignmentResults_AssignmentOptions_SelectedOptionID",
                table: "UserAssignmentResults");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAssignmentResults_AssignmentOptions_SelectedOptionID",
                table: "UserAssignmentResults",
                column: "SelectedOptionID",
                principalTable: "AssignmentOptions",
                principalColumn: "OptionID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
