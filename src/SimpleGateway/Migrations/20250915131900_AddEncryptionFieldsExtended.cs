using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddEncryptionFieldsExtended : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedPrivateKey",
                table: "EncryptionKeys");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "EncryptionKeys");

            migrationBuilder.RenameColumn(
                name: "PublicKey",
                table: "EncryptionKeys",
                newName: "Key");

            migrationBuilder.AddColumn<DateTime>(
                name: "EncryptionDisabledAt",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EncryptionEnabledAt",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastKeyRotation",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedAt",
                table: "EncryptionKeys",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "EncryptionKeys",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EncryptionKeys_IsActive",
                table: "EncryptionKeys",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EncryptionKeys_UserId_IsActive",
                table: "EncryptionKeys",
                columns: new[] { "UserId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EncryptionKeys_IsActive",
                table: "EncryptionKeys");

            migrationBuilder.DropIndex(
                name: "IX_EncryptionKeys_UserId_IsActive",
                table: "EncryptionKeys");

            migrationBuilder.DropColumn(
                name: "EncryptionDisabledAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EncryptionEnabledAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastKeyRotation",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeactivatedAt",
                table: "EncryptionKeys");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "EncryptionKeys");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "EncryptionKeys",
                newName: "PublicKey");

            migrationBuilder.AddColumn<string>(
                name: "EncryptedPrivateKey",
                table: "EncryptionKeys",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "EncryptionKeys",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
