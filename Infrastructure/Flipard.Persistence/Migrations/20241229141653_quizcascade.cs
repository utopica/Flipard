using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flipard.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class quizcascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizAnswers_Vocabularies_VocabularyId",
                table: "QuizAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_Decks_DeckId",
                table: "QuizAttempts");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAnswers_Vocabularies_VocabularyId",
                table: "QuizAnswers",
                column: "VocabularyId",
                principalTable: "Vocabularies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_Decks_DeckId",
                table: "QuizAttempts",
                column: "DeckId",
                principalTable: "Decks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizAnswers_Vocabularies_VocabularyId",
                table: "QuizAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_Decks_DeckId",
                table: "QuizAttempts");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAnswers_Vocabularies_VocabularyId",
                table: "QuizAnswers",
                column: "VocabularyId",
                principalTable: "Vocabularies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_Decks_DeckId",
                table: "QuizAttempts",
                column: "DeckId",
                principalTable: "Decks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
