using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiEducation.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ContentStudioQuizAndLeagueRollover : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "QuizQuestions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionType",
                table: "QuizQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "multiple_choice");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Lessons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NextStep",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClosedAtUtc",
                table: "LeagueSeasons",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "NextStep",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "ClosedAtUtc",
                table: "LeagueSeasons");
        }
    }
}
