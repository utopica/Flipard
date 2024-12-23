using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flipard.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizAnswers_QuizAttempts_QuizAttemptId1",
                table: "QuizAnswers");

            migrationBuilder.DropIndex(
                name: "IX_QuizAnswers_QuizAttemptId1",
                table: "QuizAnswers");

            migrationBuilder.DropColumn(
                name: "QuizAttemptId1",
                table: "QuizAnswers");

            migrationBuilder.DropColumn(
                name: "QuizAttemptId2",
                table: "QuizAnswers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "QuizAttemptId1",
                table: "QuizAnswers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "QuizAttemptId2",
                table: "QuizAnswers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswers_QuizAttemptId1",
                table: "QuizAnswers",
                column: "QuizAttemptId1");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAnswers_QuizAttempts_QuizAttemptId1",
                table: "QuizAnswers",
                column: "QuizAttemptId1",
                principalTable: "QuizAttempts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
