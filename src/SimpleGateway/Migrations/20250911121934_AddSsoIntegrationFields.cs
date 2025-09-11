using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddSsoIntegrationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSsoUser",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SsoProvider",
                table: "Users",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SsoUsername",
                table: "Users",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SsoConfigs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ServerUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    BaseDn = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    BindDn = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    BindPassword = table.Column<string>(type: "TEXT", nullable: false),
                    UserSearchFilter = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    GroupSearchFilter = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SsoConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SsoConfigs_Provider",
                table: "SsoConfigs",
                column: "Provider",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SsoConfigs");

            migrationBuilder.DropColumn(
                name: "IsSsoUser",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SsoProvider",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SsoUsername",
                table: "Users");
        }
    }
}
