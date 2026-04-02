using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engooru.Migrations
{
    /// <inheritdoc />
    public partial class AddResetPasswordFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsResetEmailVerified",
                table: "VerificationCodes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsResetMobileVerified",
                table: "VerificationCodes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ResetEmailOtp",
                table: "VerificationCodes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResetMobileOtp",
                table: "VerificationCodes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetOtpExpiry",
                table: "VerificationCodes",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsResetEmailVerified",
                table: "VerificationCodes");

            migrationBuilder.DropColumn(
                name: "IsResetMobileVerified",
                table: "VerificationCodes");

            migrationBuilder.DropColumn(
                name: "ResetEmailOtp",
                table: "VerificationCodes");

            migrationBuilder.DropColumn(
                name: "ResetMobileOtp",
                table: "VerificationCodes");

            migrationBuilder.DropColumn(
                name: "ResetOtpExpiry",
                table: "VerificationCodes");
        }
    }
}
