using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProBeacon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProjectMemberRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "ProjectMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Preserve existing access: editors (CanEdit) -> Editor(1), everyone else -> Viewer(0).
            // (Every prior member had CanView = true, so no rows are lost.)
            migrationBuilder.Sql(
                @"UPDATE ""ProjectMembers"" SET ""Role"" = 1 WHERE ""CanEdit"" = true;");

            migrationBuilder.DropColumn(
                name: "CanEdit",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "CanView",
                table: "ProjectMembers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanEdit",
                table: "ProjectMembers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanView",
                table: "ProjectMembers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Editor/Manager (Role >= 1) regain edit; all members regain view.
            migrationBuilder.Sql(
                @"UPDATE ""ProjectMembers"" SET ""CanView"" = true, ""CanEdit"" = (""Role"" >= 1);");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "ProjectMembers");
        }
    }
}
