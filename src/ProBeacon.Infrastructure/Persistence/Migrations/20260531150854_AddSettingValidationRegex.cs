using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProBeacon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingValidationRegex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ValidationRegex",
                table: "TenantSettings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValidationRegex",
                table: "TenantSettings");
        }
    }
}
