using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class FixProfileSchemaName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "profile");

            migrationBuilder.RenameTable(
                name: "wishlists",
                schema: "profiles",
                newName: "wishlists",
                newSchema: "profile");

            migrationBuilder.RenameTable(
                name: "wished_items",
                schema: "profiles",
                newName: "wished_items",
                newSchema: "profile");

            migrationBuilder.RenameTable(
                name: "user_profiles",
                schema: "profiles",
                newName: "user_profiles",
                newSchema: "profile");

            migrationBuilder.RenameTable(
                name: "addresses",
                schema: "profiles",
                newName: "addresses",
                newSchema: "profile");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "profiles");

            migrationBuilder.RenameTable(
                name: "wishlists",
                schema: "profile",
                newName: "wishlists",
                newSchema: "profiles");

            migrationBuilder.RenameTable(
                name: "wished_items",
                schema: "profile",
                newName: "wished_items",
                newSchema: "profiles");

            migrationBuilder.RenameTable(
                name: "user_profiles",
                schema: "profile",
                newName: "user_profiles",
                newSchema: "profiles");

            migrationBuilder.RenameTable(
                name: "addresses",
                schema: "profile",
                newName: "addresses",
                newSchema: "profiles");
        }
    }
}
