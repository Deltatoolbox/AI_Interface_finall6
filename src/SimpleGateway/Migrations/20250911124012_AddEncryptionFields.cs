using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddEncryptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EncryptionEnabled",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "KeyRotationDays",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 90);

            migrationBuilder.AddColumn<string>(
                name: "EncryptionKeyId",
                table: "Messages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEncrypted",
                table: "Messages",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Iv",
                table: "Messages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "Messages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EncryptionKeys",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    PublicKey = table.Column<string>(type: "TEXT", nullable: false),
                    EncryptedPrivateKey = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncryptionKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EncryptionKeys_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_EncryptionKeyId",
                table: "Messages",
                column: "EncryptionKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_EncryptionKeys_UserId",
                table: "EncryptionKeys",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_EncryptionKeys_EncryptionKeyId",
                table: "Messages",
                column: "EncryptionKeyId",
                principalTable: "EncryptionKeys",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_EncryptionKeys_EncryptionKeyId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "EncryptionKeys");

            migrationBuilder.DropIndex(
                name: "IX_Messages_EncryptionKeyId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "EncryptionEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KeyRotationDays",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EncryptionKeyId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsEncrypted",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Iv",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Tag",
                table: "Messages");
        }
    }
}
