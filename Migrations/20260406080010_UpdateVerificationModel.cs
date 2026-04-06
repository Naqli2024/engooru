using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engooru.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVerificationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "VerificationCodes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "VerificationCodes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "VerificationCodes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Profile",
                table: "VerificationCodes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "VerificationCodes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "VerificationCodes");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "VerificationCodes");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "VerificationCodes");

            migrationBuilder.DropColumn(
                name: "Profile",
                table: "VerificationCodes");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "VerificationCodes");
        }
    }
}
